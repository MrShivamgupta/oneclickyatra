namespace OneClickYatra.Api.Services.WhatsApp;

/// <summary>Parsed result of an inbound Meta webhook call, once its signature has been verified.
/// MessageId is Meta's own message id (wamid.*), used upstream for idempotent logging.</summary>
public sealed record WebhookVerificationResult(
    bool IsValid,
    string? FromPhoneNumber,
    string? MessageBody,
    string? MessageId);

/// <summary>Thrown when a call to the Meta Graph API fails (non-success HTTP status, malformed response).</summary>
public sealed class WhatsAppGatewayException : Exception
{
    public WhatsAppGatewayException(string __message) : base(__message)
    {
    }
}

/// <summary>Thrown when a WhatsApp send/webhook operation is attempted without the gateway being
/// configured (no Meta credentials set) — mapped by the exception middleware like any other
/// well-known exception, never leaking configuration details to the client.</summary>
public sealed class WhatsAppNotConfiguredException : Exception
{
    public WhatsAppNotConfiguredException()
        : base("The WhatsApp Business gateway is not configured on this environment.")
    {
    }
}
