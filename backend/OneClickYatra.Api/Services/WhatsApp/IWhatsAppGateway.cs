namespace OneClickYatra.Api.Services.WhatsApp;

/// <summary>
/// Abstraction over the WhatsApp Business gateway (Meta's own WhatsApp Cloud API, the only
/// provider — there is no third-party alternative the way Stripe is to Razorpay). Every method
/// that talks to Meta is async and can throw <see cref="WhatsAppNotConfiguredException"/> if no
/// credentials are set for this environment.
/// </summary>
public interface IWhatsAppGateway
{
    /// <summary>Sends a pre-approved template message via the Meta Graph API
    /// (POST /{PhoneNumberId}/messages) and returns Meta's own message id (wamid.*).</summary>
    Task<string> SendTemplateMessageAsync(string __toPhoneNumber, string __templateName, string[] __parameters, CancellationToken __cancellationToken);

    /// <summary>Verifies the X-Hub-Signature-256 HMAC Meta sends with every webhook call against the
    /// raw request body, and parses the first inbound message out of the payload if valid. Never
    /// trust a webhook payload before this returns IsValid = true.</summary>
    WebhookVerificationResult VerifyWebhookSignature(string __rawBody, string? __signatureHeader);

    /// <summary>Answers Meta's GET webhook-subscription handshake: true only when mode is
    /// "subscribe" and the verify token matches the one configured for this environment.</summary>
    bool VerifyWebhookChallenge(string? __mode, string? __verifyToken);
}
