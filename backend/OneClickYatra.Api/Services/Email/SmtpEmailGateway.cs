using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace OneClickYatra.Api.Services.Email;

/// <summary>
/// Real SMTP integration via MailKit (never the obsolete System.Net.Mail.SmtpClient). Requires
/// Notifications:Email:SmtpHost to be configured -- without it, SendAsync throws
/// EmailNotConfiguredException rather than attempting a connection that could not succeed.
/// </summary>
public sealed class SmtpEmailGateway : IEmailGateway
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailGateway> _logger;

    public SmtpEmailGateway(IOptions<EmailOptions> __options, ILogger<SmtpEmailGateway> __logger)
    {
        _options = __options.Value;
        _logger = __logger;
    }

    public async Task SendAsync(string __toAddress, string __subject, string __bodyHtml, CancellationToken __cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            throw new EmailNotConfiguredException();
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(__toAddress));
        message.Subject = __subject;
        message.Body = new TextPart(TextFormat.Html) { Text = __bodyHtml };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable, __cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, __cancellationToken);
            }

            await client.SendAsync(message, __cancellationToken);
            await client.DisconnectAsync(true, __cancellationToken);
        }
        catch (Exception exception)
        {
            // Never log _options.SmtpPassword -- only the host/port/recipient and the underlying
            // MailKit/SMTP failure reason.
            _logger.LogWarning(exception, "Email send via {SmtpHost}:{SmtpPort} failed for recipient {ToAddress}", _options.SmtpHost, _options.SmtpPort, __toAddress);
            throw new EmailGatewayException($"SMTP send failed: {exception.Message}");
        }
    }
}
