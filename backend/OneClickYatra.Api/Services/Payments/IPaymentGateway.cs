namespace OneClickYatra.Api.Services.Payments;

/// <summary>
/// Abstraction over the payment gateway (Razorpay primary; a Stripe implementation can be added
/// later behind this same interface without touching PaymentAppFunction). Every method that talks
/// to a real gateway is async and can throw <see cref="PaymentGatewayNotConfiguredException"/> if
/// no API keys are set for this environment.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>The gateway's public key id — safe to hand to the frontend checkout widget, never the secret.</summary>
    string PublicKeyId { get; }

    Task<GatewayOrderResult> CreateOrderAsync(decimal __amount, string __currency, string __receiptId, CancellationToken __cancellationToken);

    /// <summary>Verifies the HMAC signature Razorpay sends with every webhook call against the raw request body.
    /// Never trust a webhook payload before this returns IsValid = true.</summary>
    WebhookVerificationResult VerifyWebhookSignature(string __rawBody, string? __signatureHeader);

    Task<GatewayRefundResult> CreateRefundAsync(string __gatewayPaymentId, decimal __amount, string? __notes, CancellationToken __cancellationToken);
}
