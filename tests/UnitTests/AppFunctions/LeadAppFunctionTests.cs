using Microsoft.Extensions.Logging;
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
        _auditLogWriter.Object,
        Mock.Of<ILogger<LeadAppFunction>>());

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

    // --- Per-record ownership: a non-SuperAdmin may only manage a lead assigned to them ---

    [Fact]
    public async Task UpdateStatusAsync_LeadAssignedToSomeoneElse_ThrowsForbidden()
    {
        var caller = Guid.NewGuid();
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.UpdateStatusAsync(lead.Id, new LeadStatusRequest { Status = "Contacted" }, CancellationToken.None));
        _leadRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_LeadAssignedToCaller_Succeeds()
    {
        var caller = Guid.NewGuid();
        var lead = CreateLead();
        lead.AssignedToUserId = caller;
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await sut.UpdateStatusAsync(lead.Id, new LeadStatusRequest { Status = "Contacted" }, CancellationToken.None);

        _leadRepository.Verify(r => r.UpdateStatusAsync(lead.Id, "Contacted", caller, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_SuperAdmin_BypassesOwnershipOnAnyLead()
    {
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid(); // assigned to someone else entirely
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(true);

        var sut = CreateSut();
        await sut.UpdateStatusAsync(lead.Id, new LeadStatusRequest { Status = "Contacted" }, CancellationToken.None);

        _leadRepository.Verify(r => r.UpdateStatusAsync(lead.Id, "Contacted", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_LeadAssignedToSomeoneElse_ThrowsForbidden_AndNeverPersists()
    {
        var caller = Guid.NewGuid();
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        var request = new LeadRequest { CustomerName = "New Name", Mobile = "+919812300009" };
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.UpdateAsync(lead.Id, request, CancellationToken.None));
        _leadRepository.Verify(r => r.UpdateAsync(It.IsAny<LeadModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NeverChangesAssignedToUserId_RegardlessOfRequestBody()
    {
        // Regression test: UpdateAsync used to also write __request.AssignedToUserId onto the lead,
        // and since the "Edit Details" form never sends that field, every basic-details edit silently
        // unassigned the lead. Assignment is exclusively AssignAsync's job now.
        var caller = Guid.NewGuid();
        var lead = CreateLead();
        lead.AssignedToUserId = caller;
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        var request = new LeadRequest { CustomerName = "New Name", Mobile = "+919812300009", AssignedToUserId = null };
        await sut.UpdateAsync(lead.Id, request, CancellationToken.None);

        _leadRepository.Verify(r => r.UpdateAsync(It.Is<LeadModel>(l => l.AssignedToUserId == caller), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConvertToCustomerAsync_LeadAssignedToSomeoneElse_ThrowsForbidden()
    {
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(Guid.NewGuid());
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.ConvertToCustomerAsync(lead.Id, CancellationToken.None));
        _customerRepository.Verify(r => r.CreateAsync(It.IsAny<CustomerModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_LeadAssignedToSomeoneElse_ThrowsForbidden()
    {
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(Guid.NewGuid());
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.DeleteAsync(lead.Id, CancellationToken.None));
        _leadRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- AssignAsync: non-SuperAdmin can only claim an unassigned lead for themselves ---

    [Fact]
    public async Task AssignAsync_NonSuperAdmin_UnassignedLead_ClaimingForSelf_Succeeds()
    {
        var caller = Guid.NewGuid();
        var lead = CreateLead(); // AssignedToUserId left null by CreateLead()
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _userRepository.Setup(r => r.GetByIdAsync(caller, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = caller });
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await sut.AssignAsync(lead.Id, new LeadAssignRequest { AssignedToUserId = caller }, CancellationToken.None);

        _leadRepository.Verify(r => r.AssignAsync(lead.Id, caller, caller, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_NonSuperAdmin_UnassignedLead_AssigningToSomeoneElse_ThrowsForbidden()
    {
        var caller = Guid.NewGuid();
        var colleague = Guid.NewGuid();
        var lead = CreateLead();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _userRepository.Setup(r => r.GetByIdAsync(colleague, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = colleague });
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.AssignAsync(lead.Id, new LeadAssignRequest { AssignedToUserId = colleague }, CancellationToken.None));
        _leadRepository.Verify(r => r.AssignAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignAsync_NonSuperAdmin_AlreadyAssignedLead_ThrowsForbidden_EvenWhenClaimingForSelf()
    {
        var caller = Guid.NewGuid();
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid(); // already assigned to someone
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _userRepository.Setup(r => r.GetByIdAsync(caller, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = caller });
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.AssignAsync(lead.Id, new LeadAssignRequest { AssignedToUserId = caller }, CancellationToken.None));
        _leadRepository.Verify(r => r.AssignAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignAsync_SuperAdmin_CanReassignAnAlreadyAssignedLeadToAnyone()
    {
        var target = Guid.NewGuid();
        var lead = CreateLead();
        lead.AssignedToUserId = Guid.NewGuid();
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _userRepository.Setup(r => r.GetByIdAsync(target, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = target });
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(true);

        var sut = CreateSut();
        await sut.AssignAsync(lead.Id, new LeadAssignRequest { AssignedToUserId = target }, CancellationToken.None);

        _leadRepository.Verify(r => r.AssignAsync(lead.Id, target, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- CreateAsync: a non-SuperAdmin can only hand a new lead to themselves or leave it unassigned ---

    [Fact]
    public async Task CreateAsync_NonSuperAdmin_AssigningNewLeadToSomeoneElse_ThrowsForbidden()
    {
        var caller = Guid.NewGuid();
        var colleague = Guid.NewGuid();
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);
        _userRepository.Setup(r => r.GetByIdAsync(colleague, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = colleague });

        var sut = CreateSut();
        var request = new LeadRequest { CustomerName = "Test", Mobile = "+919812300000", AssignedToUserId = colleague };
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.CreateAsync(request, CancellationToken.None));
        _leadRepository.Verify(r => r.CreateAsync(It.IsAny<LeadModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NonSuperAdmin_AssigningNewLeadToSelf_Succeeds()
    {
        var caller = Guid.NewGuid();
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);
        _userRepository.Setup(r => r.GetByIdAsync(caller, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = caller });
        _leadRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new LeadModel { Id = id, CustomerName = "Test", Mobile = "+919812300000", AssignedToUserId = caller });

        var sut = CreateSut();
        var request = new LeadRequest { CustomerName = "Test", Mobile = "+919812300000", AssignedToUserId = caller };
        await sut.CreateAsync(request, CancellationToken.None);

        _leadRepository.Verify(r => r.CreateAsync(It.Is<LeadModel>(l => l.AssignedToUserId == caller), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NonSuperAdmin_LeavingUnassigned_Succeeds()
    {
        _currentUserAccessor.Setup(a => a.UserId).Returns(Guid.NewGuid());
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);
        _leadRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new LeadModel { Id = id, CustomerName = "Test", Mobile = "+919812300000" });

        var sut = CreateSut();
        var request = new LeadRequest { CustomerName = "Test", Mobile = "+919812300000", AssignedToUserId = null };
        await sut.CreateAsync(request, CancellationToken.None);

        _leadRepository.Verify(r => r.CreateAsync(It.Is<LeadModel>(l => l.AssignedToUserId == null), It.IsAny<CancellationToken>()), Times.Once);
    }
}
