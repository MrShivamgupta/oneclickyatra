namespace OneClickYatra.Api.Services.Email;

/// <summary>
/// Abstraction over the outbound email (SMTP) gateway. The only implementation is
/// <see cref="SmtpEmailGateway"/> (MailKit) -- there is no second provider to plan for the way
/// Razorpay is one of several possible payment providers.
/// </summary>
public interface IEmailGateway
{
    /// <summary>Sends an HTML email and throws <see cref="EmailNotConfiguredException"/> if no SMTP
    /// host is configured for this environment, or <see cref="EmailGatewayException"/> if the send
    /// itself fails.</summary>
    Task SendAsync(string __toAddress, string __subject, string __bodyHtml, CancellationToken __cancellationToken);
}
