using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace OneClickYatra.Api.Services.WhatsApp;

/// <summary>
/// Real Meta WhatsApp Business Cloud API integration (send template message, and inbound-webhook
/// signature verification) per https://developers.facebook.com/docs/whatsapp/cloud-api/ — the only
/// provider Meta offers for the WhatsApp Business Platform, so unlike Payments there is no second
/// IWhatsAppGateway implementation to plan for. Requires
/// Notifications:WhatsApp:PhoneNumberId/AccessToken to be configured — without them,
/// SendTemplateMessageAsync throws WhatsAppNotConfiguredException rather than attempting a call
/// that would fail with an opaque 401 from Meta.
/// </summary>
public sealed class MetaWhatsAppGateway : IWhatsAppGateway
{
    private const string BaseUrl = "https://graph.facebook.com/v18.0/";
    private const string LanguageCode = "en_US";

    private readonly HttpClient _httpClient;
    private readonly WhatsAppOptions _options;
    private readonly ILogger<MetaWhatsAppGateway> _logger;

    public MetaWhatsAppGateway(HttpClient __httpClient, IOptions<WhatsAppOptions> __options, ILogger<MetaWhatsAppGateway> __logger)
    {
        _httpClient = __httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _options = __options.Value;
        _logger = __logger;
    }

    public async Task<string> SendTemplateMessageAsync(string __toPhoneNumber, string __templateName, string[] __parameters, CancellationToken __cancellationToken)
    {
        EnsureConfigured();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        // Shape per Meta's documented "Send Messages" template-message body:
        // https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-message-templates
        var payload = new
        {
            messaging_product = "whatsapp",
            to = __toPhoneNumber,
            type = "template",
            template = new
            {
                name = __templateName,
                language = new { code = LanguageCode },
                components = new object[]
                {
                    new
                    {
                        type = "body",
                        parameters = __parameters.Select(parameter => new { type = "text", text = parameter }).ToArray()
                    }
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync($"{_options.PhoneNumberId}/messages", payload, __cancellationToken);
        var body = await response.Content.ReadAsStringAsync(__cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var truncatedBody = body.Length > 500 ? body[..500] : body;
            _logger.LogWarning("WhatsApp template send to Graph API failed with status {StatusCode}. Response: {ResponseBody}", (int)response.StatusCode, truncatedBody);
            throw new WhatsAppGatewayException($"WhatsApp template send failed ({(int)response.StatusCode}): {body}");
        }

        var json = JsonNode.Parse(body) ?? throw new WhatsAppGatewayException("Meta returned an unparseable send-message response.");
        var messageId = json["messages"]?[0]?["id"]?.GetValue<string>()
            ?? throw new WhatsAppGatewayException("Meta send-message response had no message id.");

        return messageId;
    }

    public WebhookVerificationResult VerifyWebhookSignature(string __rawBody, string? __signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(__signatureHeader) || string.IsNullOrWhiteSpace(_options.AppSecret))
        {
            return new WebhookVerificationResult(false, null, null, null);
        }

        // Meta prefixes the header value with "sha256=", unlike Razorpay's bare hex signature.
        const string signaturePrefix = "sha256=";
        var providedHex = __signatureHeader.StartsWith(signaturePrefix, StringComparison.OrdinalIgnoreCase)
            ? __signatureHeader[signaturePrefix.Length..]
            : __signatureHeader;

        var expectedHex = HmacSha256Hex(_options.AppSecret, __rawBody);
        if (!TryFromHex(expectedHex, out var expectedBytes) || !TryFromHex(providedHex, out var actualBytes)
            || !CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes))
        {
            return new WebhookVerificationResult(false, null, null, null);
        }

        // Shape per Meta's documented inbound-message webhook payload:
        // entry[0].changes[0].value.messages[0] — { from, id, text: { body }, ... }
        var json = JsonNode.Parse(__rawBody);
        var message = json?["entry"]?[0]?["changes"]?[0]?["value"]?["messages"]?[0];
        var fromPhoneNumber = message?["from"]?.GetValue<string>();
        var messageBody = message?["text"]?["body"]?.GetValue<string>();
        var messageId = message?["id"]?.GetValue<string>();

        return new WebhookVerificationResult(true, fromPhoneNumber, messageBody, messageId);
    }

    public bool VerifyWebhookChallenge(string? __mode, string? __verifyToken)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookVerifyToken))
        {
            return false;
        }

        return string.Equals(__mode, "subscribe", StringComparison.Ordinal)
            && string.Equals(__verifyToken, _options.WebhookVerifyToken, StringComparison.Ordinal);
    }

    private void EnsureConfigured()
    {
        if (!_options.IsConfigured)
        {
            throw new WhatsAppNotConfiguredException();
        }
    }

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
