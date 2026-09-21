using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace OneClickYatra.Api.Services.Payments;

/// <summary>
/// Real Razorpay REST API integration (Orders, Refunds, and webhook signature verification) per
/// https://razorpay.com/docs/api/ — the only gateway wired up so far; Stripe (or any other) can
/// be added later as a second IPaymentGateway implementation without touching PaymentAppFunction.
/// Requires Payments:Razorpay:KeyId/KeySecret/WebhookSecret to be configured (test-mode keys are
/// free from the Razorpay dashboard and never move real money) — without them, CreateOrderAsync
/// and CreateRefundAsync throw PaymentGatewayNotConfiguredException rather than attempting a call
/// that would fail with an opaque 401 from Razorpay.
/// </summary>
public sealed class RazorpayPaymentGateway : IPaymentGateway
{
    private const string BaseUrl = "https://api.razorpay.com/v1/";

    private const int MaxLoggedResponseBodyLength = 500;

    private readonly HttpClient _httpClient;
    private readonly RazorpayOptions _options;
    private readonly ILogger<RazorpayPaymentGateway> _logger;

    public RazorpayPaymentGateway(HttpClient __httpClient, IOptions<RazorpayOptions> __options, ILogger<RazorpayPaymentGateway> __logger)
    {
        _httpClient = __httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _options = __options.Value;
        _logger = __logger;
    }

    public string PublicKeyId => _options.KeyId;

    public async Task<GatewayOrderResult> CreateOrderAsync(decimal __amount, string __currency, string __receiptId, CancellationToken __cancellationToken)
    {
        EnsureConfigured();
        ApplyBasicAuth();

        var payload = new { amount = ToSmallestUnit(__amount), currency = __currency, receipt = __receiptId };
        using var response = await _httpClient.PostAsJsonAsync("orders", payload, __cancellationToken);
        var body = await response.Content.ReadAsStringAsync(__cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Razorpay order creation failed. StatusCode: {StatusCode}, ResponseBody: {ResponseBody}",
                (int)response.StatusCode, Truncate(body));
            throw new PaymentGatewayException($"Razorpay order creation failed ({(int)response.StatusCode}): {body}");
        }

        var json = JsonNode.Parse(body) ?? throw new PaymentGatewayException("Razorpay returned an unparseable order response.");
        var orderId = json["id"]?.GetValue<string>() ?? throw new PaymentGatewayException("Razorpay order response had no id.");
        var status = json["status"]?.GetValue<string>() ?? "created";

        return new GatewayOrderResult(orderId, status);
    }

    public WebhookVerificationResult VerifyWebhookSignature(string __rawBody, string? __signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(__signatureHeader) || string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            return new WebhookVerificationResult(false, null, null, null, null);
        }

        var expected = HmacSha256Hex(_options.WebhookSecret, __rawBody);
        if (!TryFromHex(expected, out var expectedBytes) || !TryFromHex(__signatureHeader, out var actualBytes)
            || !CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            return new WebhookVerificationResult(false, null, null, null, null);
        }

        var json = JsonNode.Parse(__rawBody);
        var eventType = json?["event"]?.GetValue<string>();
        var paymentEntity = json?["payload"]?["payment"]?["entity"];
        var orderId = paymentEntity?["order_id"]?.GetValue<string>();
        var paymentId = paymentEntity?["id"]?.GetValue<string>();
        var amountPaise = paymentEntity?["amount"]?.GetValue<long>();
        decimal? amountInRupees = amountPaise.HasValue ? amountPaise.Value / 100m : null;

        return new WebhookVerificationResult(true, eventType, orderId, paymentId, amountInRupees);
    }

    public async Task<GatewayRefundResult> CreateRefundAsync(string __gatewayPaymentId, decimal __amount, string? __notes, CancellationToken __cancellationToken)
    {
        EnsureConfigured();
        ApplyBasicAuth();

        var payload = new { amount = ToSmallestUnit(__amount), notes = new { reason = __notes ?? string.Empty } };
        using var response = await _httpClient.PostAsJsonAsync($"payments/{__gatewayPaymentId}/refund", payload, __cancellationToken);
        var body = await response.Content.ReadAsStringAsync(__cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Razorpay refund failed. StatusCode: {StatusCode}, ResponseBody: {ResponseBody}",
                (int)response.StatusCode, Truncate(body));
            throw new PaymentGatewayException($"Razorpay refund failed ({(int)response.StatusCode}): {body}");
        }

        var json = JsonNode.Parse(body) ?? throw new PaymentGatewayException("Razorpay returned an unparseable refund response.");
        var refundId = json["id"]?.GetValue<string>() ?? throw new PaymentGatewayException("Razorpay refund response had no id.");
        var status = json["status"]?.GetValue<string>() ?? "processing";

        return new GatewayRefundResult(refundId, status);
    }

    private void EnsureConfigured()
    {
        if (!_options.IsConfigured)
        {
            throw new PaymentGatewayNotConfiguredException("Razorpay");
        }
    }

    private void ApplyBasicAuth()
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.KeyId}:{_options.KeySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    private static long ToSmallestUnit(decimal __amountInRupees) => (long)Math.Round(__amountInRupees * 100m, MidpointRounding.AwayFromZero);

    private static string Truncate(string __value) =>
        __value.Length <= MaxLoggedResponseBodyLength ? __value : __value[..MaxLoggedResponseBodyLength];

    private static string HmacSha256Hex(string __secret, string __message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(__secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(__message));
        return Convert.ToHexStringLower(hash);
    }

    private static bool TryFromHex(string __value, out byte[] __bytes)
    {
        try
        {
            __bytes = Convert.FromHexString(__value);
            return true;
        }
        catch (FormatException)
        {
            __bytes = [];
            return false;
        }
    }
}
