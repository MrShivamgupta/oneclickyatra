using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.UnitTests.AppFunctions;

public class QuotationAppFunctionTests
{
    private readonly Mock<IQuotationRepository> _quotationRepository = new();
    private readonly Mock<ILeadRepository> _leadRepository = new();
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<IPackageRepository> _packageRepository = new();
    private readonly Mock<IQuotationPdfService> _pdfService = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();
    private readonly Mock<IEmailNotificationSender> _emailNotificationSender = new();

    private QuotationAppFunction CreateSut() => new(
        _quotationRepository.Object,
        _leadRepository.Object,
        _destinationRepository.Object,
        _packageRepository.Object,
        _pdfService.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object,
        _emailNotificationSender.Object,
        Mock.Of<ILogger<QuotationAppFunction>>());

    private static QuotationModel CreateQuotation(string status = "Draft") => new()
    {
        Id = Guid.NewGuid(),
        QuotationNumber = "QT-20260101-ABCDEF",
        LeadId = Guid.NewGuid(),
        Title = "Goa Family Trip",
        Status = status,
        PublicTokenHash = "hash"
    };

    [Fact]
    public async Task CreateAsync_UnknownLeadId_ThrowsEntityNotFound()
    {
        var leadId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(leadId, It.IsAny<CancellationToken>())).ReturnsAsync((LeadModel?)null);

        var sut = CreateSut();
        var request = new QuotationRequest { LeadId = leadId, Title = "Test" };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.CreateAsync(request, CancellationToken.None));
        _quotationRepository.Verify(r => r.CreateAsync(It.IsAny<QuotationModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidLead_GeneratesPublicTokenReturnedOnlyOnce()
    {
        var lead = new LeadModel { Id = Guid.NewGuid(), CustomerName = "Rahul Verma", Mobile = "+919812300001" };
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);

        var sut = CreateSut();
        var result = await sut.CreateAsync(new QuotationRequest { LeadId = lead.Id, Title = "Goa Trip" }, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.PublicToken));
        Assert.StartsWith("QT-", result.QuotationNumber);
        _quotationRepository.Verify(r => r.CreateAsync(
            It.Is<QuotationModel>(q => q.PublicTokenHash == TokenHasher.Hash(result.PublicToken!)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_StatusNotDraft_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Sent");
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.SendAsync(quotation.Id, CancellationToken.None));
        _quotationRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAsync_NoOptions_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Draft");
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);
        _quotationRepository.Setup(r => r.GetOptionsAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.SendAsync(quotation.Id, CancellationToken.None));
    }

    [Fact]
    public async Task ReplaceOptionsAsync_UnknownDestinationId_ThrowsEntityNotFound()
    {
        var quotation = CreateQuotation(status: "Draft");
        var destinationId = Guid.NewGuid();
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);
        _destinationRepository.Setup(r => r.GetExistingIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut();
        var options = new List<QuotationOptionRequest>
        {
            new() { OptionName = "Standard", DestinationId = destinationId, PricePerPerson = 10000, NumberOfPeople = 2 }
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.ReplaceOptionsAsync(quotation.Id, options, CancellationToken.None));
        _quotationRepository.Verify(r => r.ReplaceOptionsAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyList<(QuotationOptionModel, List<QuotationItemModel>)>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReplaceOptionsAsync_QuotationAlreadyApproved_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Approved");
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        var sut = CreateSut();
        var options = new List<QuotationOptionRequest> { new() { OptionName = "Standard", PricePerPerson = 10000, NumberOfPeople = 2 } };

        await Assert.ThrowsAsync<BusinessException>(() => sut.ReplaceOptionsAsync(quotation.Id, options, CancellationToken.None));
    }

    [Fact]
    public async Task ReplaceOptionsAsync_ComputesTotalPriceServerSide()
    {
        var quotation = CreateQuotation(status: "Draft");
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        IReadOnlyList<(QuotationOptionModel Option, List<QuotationItemModel> Items)>? captured = null;
        _quotationRepository
            .Setup(r => r.ReplaceOptionsAsync(quotation.Id, It.IsAny<IReadOnlyList<(QuotationOptionModel, List<QuotationItemModel>)>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, IReadOnlyList<(QuotationOptionModel, List<QuotationItemModel>)>, CancellationToken>((_, options, _) => captured = options)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var request = new List<QuotationOptionRequest> { new() { OptionName = "Standard", PricePerPerson = 15000, NumberOfPeople = 4 } };

        var result = await sut.ReplaceOptionsAsync(quotation.Id, request, CancellationToken.None);

        Assert.Equal(60000, result[0].TotalPrice);
        Assert.Equal(60000, captured?[0].Option.TotalPrice);
    }

    [Fact]
    public async Task ApprovePublicAsync_QuotationNotSent_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Approved");
        _quotationRepository.Setup(r => r.GetByPublicTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        var sut = CreateSut();
        var request = new QuotationPublicApproveRequest { SelectedOptionId = Guid.NewGuid(), ApprovedByName = "Rahul" };

        await Assert.ThrowsAsync<BusinessException>(() => sut.ApprovePublicAsync("raw-token", request, "127.0.0.1", CancellationToken.None));
        _quotationRepository.Verify(r => r.ApproveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApprovePublicAsync_OptionBelongsToDifferentQuotation_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Sent");
        var foreignOption = new QuotationOptionModel { Id = Guid.NewGuid(), QuotationId = Guid.NewGuid(), OptionName = "Other" };
        _quotationRepository.Setup(r => r.GetByPublicTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(quotation);
        _quotationRepository.Setup(r => r.GetOptionByIdAsync(foreignOption.Id, It.IsAny<CancellationToken>())).ReturnsAsync(foreignOption);

        var sut = CreateSut();
        var request = new QuotationPublicApproveRequest { SelectedOptionId = foreignOption.Id, ApprovedByName = "Rahul" };

        await Assert.ThrowsAsync<BusinessException>(() => sut.ApprovePublicAsync("raw-token", request, "127.0.0.1", CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_QuotationAlreadySent_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Sent");
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.DeleteAsync(quotation.Id, CancellationToken.None));
        _quotationRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_QuotationAlreadyApproved_ThrowsBusiness()
    {
        var quotation = CreateQuotation(status: "Approved");
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.UpdateAsync(quotation.Id, new QuotationRequest { LeadId = quotation.LeadId, Title = "New Title" }, CancellationToken.None));
    }
}
