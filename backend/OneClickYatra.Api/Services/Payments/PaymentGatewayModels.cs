namespace OneClickYatra.Api.Services.Payments;

public sealed record GatewayOrderResult(string GatewayOrderId, string Status);

public sealed record WebhookVerificationResult(
    bool IsValid,
    string? EventType,
    string? GatewayOrderId,
    string? GatewayPaymentId,
    decimal? AmountInRupees);

public sealed record GatewayRefundResult(string GatewayRefundId, string Status);

/// <summary>Thrown when a gateway operation is attempted without the provider being configured
/// (no API keys set) — mapped by the exception middleware like any other well-known exception,
/// never leaking configuration details to the client.</summary>
public sealed class PaymentGatewayNotConfiguredException : Exception
{
    public PaymentGatewayNotConfiguredException(string __provider)
        : base($"The {__provider} payment gateway is not configured on this environment.")
    {
    }
}
