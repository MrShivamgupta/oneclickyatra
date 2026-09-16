using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class EnquiryAppFunctionTests
{
    private readonly Mock<IEnquiryRepository> _enquiryRepository = new();
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private EnquiryAppFunction CreateSut() => new(
        _enquiryRepository.Object,
        _destinationRepository.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object);

    private static EnquiryModel CreateEnquiry(string status = "New") => new()
    {
        Id = Guid.NewGuid(),
        FullName = "Jane Doe",
        Email = "jane@example.com",
        Phone = "+911234567890",
        Message = "Looking for a honeymoon package.",
        Status = status
    };

    [Fact]
    public async Task CreateAsync_UnknownDestinationId_ThrowsEntityNotFoundException()
    {
        var destinationId = Guid.NewGuid();
        _destinationRepository.Setup(r => r.GetByIdAsync(destinationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DestinationModel?)null);

        var sut = CreateSut();
        var request = new EnquiryRequest
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+911234567890",
            DestinationId = destinationId,
            Message = "Looking for a honeymoon package."
        };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.CreateAsync(request, CancellationToken.None));

        _enquiryRepository.Verify(r => r.CreateAsync(It.IsAny<EnquiryModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequestWithNoDestination_CreatesWithStatusNewAndNullCreatedBy()
    {
        _currentUserAccessor.Setup(a => a.UserId).Returns((Guid?)null);

        EnquiryModel? saved = null;
        _enquiryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid id, CancellationToken _) => Task.FromResult(saved));
        _enquiryRepository
            .Setup(r => r.CreateAsync(It.IsAny<EnquiryModel>(), It.IsAny<CancellationToken>()))
            .Callback((EnquiryModel enquiry, CancellationToken _) => saved = enquiry)
            .ReturnsAsync((EnquiryModel enquiry, CancellationToken _) => enquiry.Id);

        var sut = CreateSut();
        var request = new EnquiryRequest
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+911234567890",
            Message = "Looking for a honeymoon package."
        };

        var result = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal("New", result.Status);
        Assert.Equal("Jane Doe", result.FullName);
        _enquiryRepository.Verify(r => r.CreateAsync(
            It.Is<EnquiryModel>(e => e.Status == "New" && e.CreatedBy == null),
            It.IsAny<CancellationToken>()), Times.Once);
        _destinationRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _enquiryRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnquiryModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.GetByIdAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidId_UpdatesStatusAndAuditLogsOldAndNewStatus()
    {
        var enquiry = CreateEnquiry("New");
        _enquiryRepository.Setup(r => r.GetByIdAsync(enquiry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(enquiry);

        var sut = CreateSut();
        var request = new EnquiryStatusRequest { Status = "Contacted" };

        await sut.UpdateStatusAsync(enquiry.Id, request, CancellationToken.None);

        _enquiryRepository.Verify(r => r.UpdateStatusAsync(enquiry.Id, "Contacted", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(), "enquiry.status.changed", "Enquiry", enquiry.Id.ToString(), "New", "Contacted", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsEntityNotFoundException()
    {
        var id = Guid.NewGuid();
        _enquiryRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnquiryModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(id, CancellationToken.None));

        _enquiryRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
