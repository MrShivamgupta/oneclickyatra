using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.UnitTests.Services;

public class EmailTemplateServiceTests
{
    private readonly Mock<IEmailTemplateRepository> _templateRepository = new();

    private EmailTemplateService CreateSut() => new(_templateRepository.Object, Mock.Of<ILogger<EmailTemplateService>>());

    private static EmailTemplateModel CreateTemplate(bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Name = "BookingConfirmation",
        Subject = "Your booking {{BookingReference}} is confirmed!",
        BodyHtml = "<p>Dear {{CustomerName}}, your package {{PackageName}} is confirmed. Ref: {{BookingReference}}. Note: {{UnknownField}}.</p>",
        IsActive = isActive
    };

    [Fact]
    public async Task RenderAsync_AllPlaceholdersProvided_SubstitutesEveryToken()
    {
        var template = CreateTemplate();
        _templateRepository.Setup(r => r.GetByNameAsync(template.Name, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var sut = CreateSut();
        var placeholders = new Dictionary<string, string>
        {
            ["CustomerName"] = "Asha",
            ["PackageName"] = "Goa Beach Package",
            ["BookingReference"] = "BK-1001",
            ["UnknownField"] = "all good"
        };

        var (subject, body) = await sut.RenderAsync(template.Name, placeholders, CancellationToken.None);

        Assert.Equal("Your booking BK-1001 is confirmed!", subject);
        Assert.Contains("Dear Asha", body);
        Assert.Contains("Goa Beach Package", body);
        Assert.Contains("Ref: BK-1001", body);
        Assert.Contains("Note: all good.", body);
    }

    [Fact]
    public async Task RenderAsync_MissingPlaceholderValue_LeavesTokenAsLiteralText()
    {
        var template = CreateTemplate();
        _templateRepository.Setup(r => r.GetByNameAsync(template.Name, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var sut = CreateSut();
        // UnknownField deliberately omitted -- should survive as the literal "{{UnknownField}}" text
        // rather than throwing or being blanked out.
        var placeholders = new Dictionary<string, string>
        {
            ["CustomerName"] = "Asha",
            ["PackageName"] = "Goa Beach Package",
            ["BookingReference"] = "BK-1001"
        };

        var (_, body) = await sut.RenderAsync(template.Name, placeholders, CancellationToken.None);

        Assert.Contains("{{UnknownField}}", body);
    }

    [Fact]
    public async Task RenderAsync_TemplateDoesNotExist_ThrowsEntityNotFound()
    {
        _templateRepository.Setup(r => r.GetByNameAsync("MissingTemplate", It.IsAny<CancellationToken>())).ReturnsAsync((EmailTemplateModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.RenderAsync("MissingTemplate", new Dictionary<string, string>(), CancellationToken.None));
    }

    [Fact]
    public async Task RenderAsync_TemplateInactive_ThrowsEntityNotFound()
    {
        var template = CreateTemplate(isActive: false);
        _templateRepository.Setup(r => r.GetByNameAsync(template.Name, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.RenderAsync(template.Name, new Dictionary<string, string>(), CancellationToken.None));
    }
}
