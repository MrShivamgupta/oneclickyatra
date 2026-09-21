namespace OneClickYatra.Api.Services.WhatsApp;

public sealed class WhatsAppOptions
{
    public const string SectionName = "Notifications:WhatsApp";

    /// <summary>The sending phone number's id, as assigned by Meta (WhatsApp Business Platform / Cloud API).</summary>
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>Permanent (system-user) access token for the Meta Graph API.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Shared secret this app expects back in the GET webhook-verification handshake's hub.verify_token.</summary>
    public string WebhookVerifyToken { get; set; } = string.Empty;

    /// <summary>The Meta App Secret used to verify X-Hub-Signature-256 on inbound webhook calls.</summary>
    public string AppSecret { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(PhoneNumberId) && !string.IsNullOrWhiteSpace(AccessToken);
}
