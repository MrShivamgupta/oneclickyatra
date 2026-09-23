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

public class FollowUpAppFunctionTests
{
    private readonly Mock<IFollowUpRepository> _followUpRepository = new();
    private readonly Mock<ILeadRepository> _leadRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private FollowUpAppFunction CreateSut() => new(
        _followUpRepository.Object,
        _leadRepository.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object,
        Mock.Of<ILogger<FollowUpAppFunction>>());

    private static LeadModel MakeLead(Guid? assignedToUserId = null) => new()
    {
        Id = Guid.NewGuid(),
        CustomerName = "Rahul Verma",
        Mobile = "+919812300001",
        Status = "New",
        AssignedToUserId = assignedToUserId
    };

    private static FollowUpModel MakeFollowUp(Guid leadId) => new()
    {
        Id = Guid.NewGuid(),
        LeadId = leadId,
        ScheduledAt = DateTime.UtcNow,
        Type = "Call",
        Status = "Pending"
    };

    // --- CreateAsync: a follow-up's ownership is its parent lead's ownership ---

    [Fact]
    public async Task CreateAsync_LeadAssignedToSomeoneElse_ThrowsForbidden()
    {
        var lead = MakeLead(assignedToUserId: Guid.NewGuid());
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(Guid.NewGuid());
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.CreateAsync(lead.Id, new FollowUpRequest { ScheduledAt = DateTime.UtcNow, Type = "Call" }, CancellationToken.None));
        _followUpRepository.Verify(r => r.CreateAsync(It.IsAny<FollowUpModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_LeadAssignedToCaller_Succeeds()
    {
        var caller = Guid.NewGuid();
        var lead = MakeLead(assignedToUserId: caller);
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);
        _followUpRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => MakeFollowUp(lead.Id));

        var sut = CreateSut();
        await sut.CreateAsync(lead.Id, new FollowUpRequest { ScheduledAt = DateTime.UtcNow, Type = "Call" }, CancellationToken.None);

        _followUpRepository.Verify(r => r.CreateAsync(It.Is<FollowUpModel>(f => f.LeadId == lead.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SuperAdmin_BypassesOwnershipOnAnyLead()
    {
        var lead = MakeLead(assignedToUserId: Guid.NewGuid());
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(true);
        _followUpRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => MakeFollowUp(lead.Id));

        var sut = CreateSut();
        await sut.CreateAsync(lead.Id, new FollowUpRequest { ScheduledAt = DateTime.UtcNow, Type = "Call" }, CancellationToken.None);

        _followUpRepository.Verify(r => r.CreateAsync(It.IsAny<FollowUpModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- UpdateStatusAsync: ownership is resolved via the follow-up's parent lead ---

    [Fact]
    public async Task UpdateStatusAsync_ParentLeadAssignedToSomeoneElse_ThrowsForbidden()
    {
        var lead = MakeLead(assignedToUserId: Guid.NewGuid());
        var followUp = MakeFollowUp(lead.Id);
        _followUpRepository.Setup(r => r.GetByIdAsync(followUp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(followUp);
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(Guid.NewGuid());
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            sut.UpdateStatusAsync(followUp.Id, new FollowUpStatusRequest { Status = "Completed" }, CancellationToken.None));
        _followUpRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ParentLeadAssignedToCaller_Succeeds()
    {
        var caller = Guid.NewGuid();
        var lead = MakeLead(assignedToUserId: caller);
        var followUp = MakeFollowUp(lead.Id);
        _followUpRepository.Setup(r => r.GetByIdAsync(followUp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(followUp);
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await sut.UpdateStatusAsync(followUp.Id, new FollowUpStatusRequest { Status = "Completed" }, CancellationToken.None);

        _followUpRepository.Verify(r => r.UpdateStatusAsync(followUp.Id, "Completed", It.IsAny<string>(), It.IsAny<DateTime?>(), caller, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DeleteAsync: same rule ---

    [Fact]
    public async Task DeleteAsync_ParentLeadAssignedToSomeoneElse_ThrowsForbidden()
    {
        var lead = MakeLead(assignedToUserId: Guid.NewGuid());
        var followUp = MakeFollowUp(lead.Id);
        _followUpRepository.Setup(r => r.GetByIdAsync(followUp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(followUp);
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.UserId).Returns(Guid.NewGuid());
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(false);

        var sut = CreateSut();
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.DeleteAsync(followUp.Id, CancellationToken.None));
        _followUpRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SuperAdmin_BypassesOwnershipOnAnyLead()
    {
        var lead = MakeLead(assignedToUserId: Guid.NewGuid());
        var followUp = MakeFollowUp(lead.Id);
        _followUpRepository.Setup(r => r.GetByIdAsync(followUp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(followUp);
        _leadRepository.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _currentUserAccessor.Setup(a => a.IsSuperAdmin).Returns(true);

        var sut = CreateSut();
        await sut.DeleteAsync(followUp.Id, CancellationToken.None);

        _followUpRepository.Verify(r => r.DeleteAsync(followUp.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
