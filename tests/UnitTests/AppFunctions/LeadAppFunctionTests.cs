using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class LeadAppFunctionTests
{
    private readonly Mock<ILeadRepository> _leadRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private LeadAppFunction CreateSut() => new(
        _leadRepository.Object,
        _customerRepository.Object,
        _destinationRepository.Object,
        _userRepository.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object);

    private static LeadModel CreateLead(Guid? customerId = null) => new()
    {
        Id = Guid.NewGuid(),
        CustomerName = "Rahul Verma",
        Mobile = "+919812300001",
        Email = "rahul@example.com",
        LeadScore = 40,
        Status = "New",
        CustomerId = customerId
    };

    [Fact]
    public async Task CreateAsync_UnknownDestinationId_ThrowsEntityNotFound()
    {
        var destinationId = Guid.NewGuid();
        _destinationRepository.Setup(r => r.GetByIdAsync(destinationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DestinationModel?)null);

        var sut = CreateSut();
        var request = new LeadRequest { CustomerName = "Test", Mobile = "+919812300000", DestinationId = destinationId };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.CreateAsync(request, CancellationToken.None));
        _leadRepository.Verify(r => r.CreateAsync(It.IsAny<LeadModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignAsync_UnknownUser_ThrowsEntityNotFound()
    {
        var lead = CreateLead();
        var userId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.AssignAsync(lead.Id, new LeadAssignRequest { AssignedToUserId = userId }, CancellationToken.None));
        _leadRepository.Verify(r => r.AssignAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConvertToCustomerAsync_NotYetConverted_CreatesCustomerAndLinksLead()
    {
        var lead = CreateLead();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);

        var sut = CreateSut();
        var result = await sut.ConvertToCustomerAsync(lead.Id, CancellationToken.None);

        Assert.Equal(lead.CustomerName, result.FullName);
        Assert.Equal(lead.Mobile, result.Phone);
        _customerRepository.Verify(r => r.CreateAsync(
            It.Is<CustomerModel>(c => c.FullName == lead.CustomerName && c.Phone == lead.Mobile),
            It.IsAny<CancellationToken>()), Times.Once);
        _leadRepository.Verify(r => r.LinkCustomerAsync(lead.Id, result.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConvertToCustomerAsync_AlreadyConverted_ReturnsExistingCustomerWithoutCreatingAnother()
    {
        var existingCustomer = new CustomerModel { Id = Guid.NewGuid(), FullName = "Rahul Verma", Phone = "+919812300001" };
        var lead = CreateLead(customerId: existingCustomer.Id);
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _customerRepository.Setup(r => r.GetByIdAsync(existingCustomer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingCustomer);

        var sut = CreateSut();
        var result = await sut.ConvertToCustomerAsync(lead.Id, CancellationToken.None);

        Assert.Equal(existingCustomer.Id, result.Id);
        _customerRepository.Verify(r => r.CreateAsync(It.IsAny<CustomerModel>(), It.IsAny<CancellationToken>()), Times.Never);
        _leadRepository.Verify(r => r.LinkCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidLead_UpdatesAndAuditLogsOldAndNewStatus()
    {
        var lead = CreateLead();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);

        var sut = CreateSut();
        await sut.UpdateStatusAsync(lead.Id, new LeadStatusRequest { Status = "Contacted" }, CancellationToken.None);

        _leadRepository.Verify(r => r.UpdateStatusAsync(lead.Id, "Contacted", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(a => a.LogAsync(
            It.IsAny<Guid?>(), "lead.status.changed", "Lead", lead.Id.ToString(), "New", "Contacted", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UnknownLead_ThrowsEntityNotFound()
    {
        var id = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((LeadModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(id, CancellationToken.None));
        _leadRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
