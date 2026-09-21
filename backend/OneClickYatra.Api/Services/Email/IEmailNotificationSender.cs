namespace OneClickYatra.Api.Services.Email;

/// <summary>
/// The single entry point every AppFunction should call to send a system email. Renders the named
/// template, sends it through <see cref="IEmailGateway"/>, and always writes a NotificationLogs
/// row -- Sent on success, Failed (with the caught gateway error) on a per-message send failure.
/// A missing SMTP configuration (<see cref="EmailNotConfiguredException"/>) is an environment
/// problem, not a per-message failure, and is left to propagate as a 503 rather than being logged
/// as a failed send.
/// </summary>
public interface IEmailNotificationSender
{
    Task SendAsync(string __templateName, string __toAddress, Dictionary<string, string> __placeholders, CancellationToken __cancellationToken);
}
