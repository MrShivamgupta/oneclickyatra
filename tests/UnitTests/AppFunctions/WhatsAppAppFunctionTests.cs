using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.WhatsApp;

namespace OneClickYatra.UnitTests.AppFunctions;

public class WhatsAppAppFunctionTests
{
    private readonly Mock<IWhatsAppTemplateRepository> _templateRepository = new();
    private readonly Mock<INotificationLogRepository> _notificationLogRepository = new();
    private readonly Mock<IWhatsAppGateway> _whatsAppGateway = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private WhatsAppAppFunction CreateSut() => new(
        _templateRepository.Object,
        _notificationLogRepository.Object,
        _whatsAppGateway.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object,
        Mock.Of<ILogger<WhatsAppAppFunction>>());

    private static WhatsAppTemplateModel CreateTemplate(string name = "quotation_ready", bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Category = "Quotation",
        BodyText = "Hi {{1}}, your quotation for {{2}} is ready. Total amount: {{3}}. View here: {{4}}",
        IsActive = isActive
    };

    [Fact]
    public async Task CreateTemplateAsync_NameAlreadyExists_ThrowsBusiness()
    {
        var existing = CreateTemplate();
        _templateRepository.Setup(r => r.GetByNameAsync(existing.Name, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var sut = CreateSut();
        var request = new WhatsAppTemplateRequest { Name = existing.Name, Category = "Quotation", BodyText = "Hi {{1}}" };

        await Assert.ThrowsAsync<BusinessException>(() => sut.CreateTemplateAsync(request, CancellationToken.None));
        _templateRepository.Verify(r => r.CreateAsync(It.IsAny<WhatsAppTemplateModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTemplateAsync_NameTakenByAnotherTemplate_ThrowsBusiness()
    {
        var template = CreateTemplate(name: "welcome_greeting");
        var other = CreateTemplate(name: "travel_alert");
        _templateRepository.Setup(r => r.GetByIdAsync(template.Id, It.IsAny<CancellationToken>())).ReturnsAsync(template);
        _templateRepository.Setup(r => r.GetByNameAsync(other.Name, It.IsAny<CancellationToken>())).ReturnsAsync(other);

        var sut = CreateSut();
        var request = new WhatsAppTemplateRequest { Name = other.Name, Category = "TravelAlert", BodyText = "Hi {{1}}" };

        await Assert.ThrowsAsync<BusinessException>(() => sut.UpdateTemplateAsync(template.Id, request, CancellationToken.None));
        _templateRepository.Verify(r => r.UpdateAsync(It.IsAny<WhatsAppTemplateModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTemplateAsync_MissingTemplate_ThrowsEntityNotFound()
    {
        var id = Guid.NewGuid();
        _templateRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((WhatsAppTemplateModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteTemplateAsync(id, CancellationToken.None));
        _templateRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_UnknownTemplate_ThrowsEntityNotFound()
    {
        _templateRepository.Setup(r => r.GetByNameAsync("missing_template", It.IsAny<CancellationToken>())).ReturnsAsync((WhatsAppTemplateModel?)null);

        var sut = CreateSut();
        var request = new WhatsAppSendMessageRequest { PhoneNumber = "+911234567890", TemplateName = "missing_template", Parameters = ["Asha"] };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.SendMessageAsync(request, CancellationToken.None));
        _whatsAppGateway.Verify(g => g.SendTemplateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_InactiveTemplate_ThrowsBusiness()
    {
        var template = CreateTemplate(isActive: false);
        _templateRepository.Setup(r => r.GetByNameAsync(template.Name, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var sut = CreateSut();
        var request = new WhatsAppSendMessageRequest { PhoneNumber = "+911234567890", TemplateName = template.Name, Parameters = ["Asha"] };

        await Assert.ThrowsAsync<BusinessException>(() => sut.SendMessageAsync(request, CancellationToken.None));
        _whatsAppGateway.Verify(g => g.SendTemplateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAsync_GatewaySucceeds_LogsSentWithGatewayMessageId()
    {
        var template = CreateTemplate();
        _templateRepository.Setup(r => r.GetByNameAsync(template.Name, It.IsAny<CancellationToken>())).ReturnsAsync(template);
        _whatsAppGateway.Setup(g => g.SendTemplateMessageAsync("+911234567890", template.Name, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("wamid.TEST123");

        var sut = CreateSut();
        var request = new WhatsAppSendMessageRequest { PhoneNumber = "+911234567890", TemplateName = template.Name, Parameters = ["Asha", "Goa Package", "25000"] };
        var result = await sut.SendMessageAsync(request, CancellationToken.None);

        Assert.Equal("Sent", result.Status);
        Assert.Equal("wamid.TEST123", result.GatewayMessageId);
        _notificationLogRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationLogModel>(log => log.Status == "Sent" && log.GatewayMessageId == "wamid.TEST123" && log.Channel == "WhatsApp"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_GatewayThrows_LogsFailedInsteadOfPropagating()
    {
        var template = CreateTemplate();
        _templateRepository.Setup(r => r.GetByNameAsync(template.Name, It.IsAny<CancellationToken>())).ReturnsAsync(template);
        _whatsAppGateway.Setup(g => g.SendTemplateMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WhatsAppGatewayException("WhatsApp template send failed (400): bad request"));

        var sut = CreateSut();
        var request = new WhatsAppSendMessageRequest { PhoneNumber = "+911234567890", TemplateName = template.Name, Parameters = ["Asha"] };
        var result = await sut.SendMessageAsync(request, CancellationToken.None);

        Assert.Equal("Failed", result.Status);
        Assert.Contains("bad request", result.ErrorMessage);
        _notificationLogRepository.Verify(r => r.CreateAsync(It.Is<NotificationLogModel>(log => log.Status == "Failed"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_InvalidSignature_ThrowsInvalidWebhookSignature()
    {
        _whatsAppGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(false, null, null, null));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidWebhookSignatureException>(() => sut.ProcessWebhookAsync("{}", "sha256=bad", CancellationToken.None));
        _notificationLogRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationLogModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_NoInboundMessage_DoesNotLog()
    {
        _whatsAppGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(true, null, null, null));

        var sut = CreateSut();
        await sut.ProcessWebhookAsync("{}", "sha256=valid", CancellationToken.None);

        _notificationLogRepository.Verify(r => r.CreateAsync(It.IsAny<NotificationLogModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ValidInboundMessage_LogsReceived()
    {
        _whatsAppGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(true, "911234567890", "Hi, is my booking confirmed?", "wamid.INBOUND1"));

        var sut = CreateSut();
        await sut.ProcessWebhookAsync("{}", "sha256=valid", CancellationToken.None);

        _notificationLogRepository.Verify(r => r.CreateAsync(
            It.Is<NotificationLogModel>(log => log.Status == "Received" && log.GatewayMessageId == "wamid.INBOUND1" && log.RecipientPhone == "911234567890"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void VerifyWebhookChallenge_DelegatesToGateway()
    {
        _whatsAppGateway.Setup(g => g.VerifyWebhookChallenge("subscribe", "configured-token")).Returns(true);

        var sut = CreateSut();

        Assert.True(sut.VerifyWebhookChallenge("subscribe", "configured-token"));
    }
}
