using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.UnitTests.Services;

public class EmailNotificationSenderTests
{
    private readonly Mock<IEmailTemplateService> _templateService = new();
    private readonly Mock<IEmailGateway> _emailGateway = new();
    private readonly Mock<INotificationLogRepository> _notificationLogRepository = new();

    private EmailNotificationSender CreateSut() => new(
        _templateService.Object,
        _emailGateway.Object,
        _notificationLogRepository.Object,
        Mock.Of<ILogger<EmailNotificationSender>>());

    private void SetUpRender(string __templateName, string __subject = "Subject", string __body = "<p>Body</p>")
    {
        _templateService.Setup(s => s.RenderAsync(__templateName, It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((__subject, __body));
    }

    [Fact]
    public async Task SendAsync_GatewaySucceeds_LogsSentWithRenderedContent()
    {
        SetUpRender("BookingConfirmation", "Your booking BK-1 is confirmed!", "<p>Dear Asha</p>");
        _emailGateway.Setup(g => g.SendAsync("asha@example.com", "Your booking BK-1 is confirmed!", "<p>Dear Asha</p>", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.SendAsync("BookingConfirmation", "asha@example.com", new Dictionary<string, string>(), CancellationToken.None);

        _notificationLogRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationLogModel>(log =>
                log.Channel == "Email"
                && log.Status == "Sent"
                && log.RecipientEmail == "asha@example.com"
                && log.Subject == "Your booking BK-1 is confirmed!"
                && log.Body == "<p>Dear Asha</p>"
                && log.SentAt != null
                && log.ErrorMessage == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_GatewayThrowsGatewayException_LogsFailedWithCaughtMessage()
    {
        SetUpRender("BookingConfirmation");
        _emailGateway.Setup(g => g.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailGatewayException("SMTP send failed: mailbox unavailable"));

        var sut = CreateSut();
        await sut.SendAsync("BookingConfirmation", "asha@example.com", new Dictionary<string, string>(), CancellationToken.None);

        _notificationLogRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationLogModel>(log =>
                log.Status == "Failed"
                && log.ErrorMessage == "SMTP send failed: mailbox unavailable"
                && log.SentAt == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_GatewayNotConfigured_PropagatesAndDoesNotLog()
    {
        SetUpRender("BookingConfirmation");
        _emailGateway.Setup(g => g.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailNotConfiguredException());

        var sut = CreateSut();

        await Assert.ThrowsAsync<EmailNotConfiguredException>(
            () => sut.SendAsync("BookingConfirmation", "asha@example.com", new Dictionary<string, string>(), CancellationToken.None));

        _notificationLogRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationLogModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
