using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.Services.Email;

public sealed class EmailNotificationSender : IEmailNotificationSender
{
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailGateway _emailGateway;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(
        IEmailTemplateService __templateService,
        IEmailGateway __emailGateway,
        INotificationLogRepository __notificationLogRepository,
        ILogger<EmailNotificationSender> __logger)
    {
        _templateService = __templateService;
        _emailGateway = __emailGateway;
        _notificationLogRepository = __notificationLogRepository;
        _logger = __logger;
    }

    /// <summary>Renders __templateName, sends it to __toAddress, and always logs the attempt --
    /// Sent on success, or Failed with the error message when the gateway call itself fails (bad
    /// recipient, SMTP server rejection, etc.). EmailNotConfiguredException is intentionally left
    /// uncaught here (only EmailGatewayException is caught) so a missing SMTP configuration bubbles
    /// up as a 503 instead of being recorded as a failed send -- mirrors
    /// WhatsAppAppFunction.SendMessageAsync's catch-only-the-gateway-exception structure.</summary>
    public async Task SendAsync(string __templateName, string __toAddress, Dictionary<string, string> __placeholders, CancellationToken __cancellationToken)
    {
        var (subject, body) = await _templateService.RenderAsync(__templateName, __placeholders, __cancellationToken);

        var log = new NotificationLogModel
        {
            Id = Guid.NewGuid(),
            Channel = "Email",
            RecipientEmail = __toAddress,
            Subject = subject,
            Body = body
        };

        try
        {
            await _emailGateway.SendAsync(__toAddress, subject, body, __cancellationToken);
            log.Status = "Sent";
            log.SentAt = DateTime.UtcNow;
        }
        catch (EmailGatewayException exception)
        {
            log.Status = "Failed";
            log.ErrorMessage = exception.Message;
            _logger.LogWarning(exception, "Email send of template {TemplateName} to {ToAddress} failed", __templateName, __toAddress);
        }

        await _notificationLogRepository.CreateAsync(log, __cancellationToken);
    }
}
