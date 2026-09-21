namespace OneClickYatra.Api.Services.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Notifications:Email";

    /// <summary>SMTP server host. Empty means the gateway is not configured for this environment.</summary>
    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string SmtpUsername { get; set; } = string.Empty;

    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>The address every outbound email is sent from.</summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>The display name paired with FromAddress.</summary>
    public string FromName { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SmtpHost);
}
