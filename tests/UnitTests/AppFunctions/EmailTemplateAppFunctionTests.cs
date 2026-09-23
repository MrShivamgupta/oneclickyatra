using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.AppFunctions;

public class EmailTemplateAppFunctionTests
{
    private readonly Mock<IEmailTemplateRepository> _templateRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();

    private EmailTemplateAppFunction CreateSut() => new(_templateRepository.Object, _currentUserAccessor.Object);

    private static EmailTemplateModel MakeTemplate(string name = "BookingConfirmation") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Subject = "Subject {{X}}",
        BodyHtml = "<p>Body {{X}}</p>",
        IsActive = true
    };

    [Fact]
    public async Task CreateAsync_NameAlreadyExists_ThrowsBusinessException()
    {
        var existing = MakeTemplate();
        _templateRepository.Setup(r => r.GetByNameAsync(existing.Name, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var sut = CreateSut();
        var request = new UpsertEmailTemplateRequest { Name = existing.Name, Subject = "New Subject", BodyHtml = "<p>New</p>" };

        var ex = await Assert.ThrowsAsync<BusinessException>(() => sut.CreateAsync(request, CancellationToken.None));
        Assert.Contains(existing.Name, ex.Message);
        _templateRepository.Verify(r => r.CreateAsync(It.IsAny<EmailTemplateModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NewName_CreatesTemplate()
    {
        _templateRepository.Setup(r => r.GetByNameAsync("CustomOne", It.IsAny<CancellationToken>())).ReturnsAsync((EmailTemplateModel?)null);
        _templateRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new EmailTemplateModel { Id = id, Name = "CustomOne", Subject = "S", BodyHtml = "<p>B</p>", IsActive = true });

        var sut = CreateSut();
        var request = new UpsertEmailTemplateRequest { Name = "CustomOne", Subject = "S", BodyHtml = "<p>B</p>", IsActive = true };
        var result = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal("CustomOne", result.Name);
        _templateRepository.Verify(r => r.CreateAsync(It.Is<EmailTemplateModel>(t => t.Name == "CustomOne"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NeverChangesName_EvenIfRequestCarriesADifferentOne()
    {
        var template = MakeTemplate("BookingConfirmation");
        _templateRepository.Setup(r => r.GetByIdAsync(template.Id, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var sut = CreateSut();
        var request = new UpsertEmailTemplateRequest { Name = "SomethingElseEntirely", Subject = "Updated subject", BodyHtml = "<p>Updated</p>", IsActive = false };
        await sut.UpdateAsync(template.Id, request, CancellationToken.None);

        _templateRepository.Verify(r => r.UpdateAsync(It.Is<EmailTemplateModel>(t =>
            t.Name == "BookingConfirmation" && t.Subject == "Updated subject" && t.BodyHtml == "<p>Updated</p>" && t.IsActive == false
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsEntityNotFound()
    {
        var id = Guid.NewGuid();
        _templateRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((EmailTemplateModel?)null);

        var sut = CreateSut();
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.UpdateAsync(id, new UpsertEmailTemplateRequest { Name = "X", Subject = "S", BodyHtml = "B" }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsEntityNotFound()
    {
        var id = Guid.NewGuid();
        _templateRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((EmailTemplateModel?)null);

        var sut = CreateSut();
        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(id, CancellationToken.None));
        _templateRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTemplate_DeletesIt()
    {
        var template = MakeTemplate();
        _templateRepository.Setup(r => r.GetByIdAsync(template.Id, It.IsAny<CancellationToken>())).ReturnsAsync(template);

        var sut = CreateSut();
        await sut.DeleteAsync(template.Id, CancellationToken.None);

        _templateRepository.Verify(r => r.DeleteAsync(template.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
