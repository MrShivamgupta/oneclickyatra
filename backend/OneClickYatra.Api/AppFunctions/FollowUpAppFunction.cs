using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class FollowUpAppFunction : IFollowUpAppFunction
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<FollowUpAppFunction> _logger;

    public FollowUpAppFunction(
        IFollowUpRepository __followUpRepository,
        ILeadRepository __leadRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ILogger<FollowUpAppFunction> __logger)
    {
        _followUpRepository = __followUpRepository;
        _leadRepository = __leadRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<IReadOnlyList<FollowUpResponse>> ListByLeadAsync(Guid __leadId, CancellationToken __cancellationToken)
    {
        var followUps = await _followUpRepository.ListByLeadAsync(__leadId, __cancellationToken);
        return followUps.Select(ToResponse).ToList();
    }

    public async Task<PaginationResponse<FollowUpResponse>> ListTodayAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _followUpRepository.ListTodayAsync(__request, __cancellationToken);
        return PaginationResponse<FollowUpResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<FollowUpResponse> CreateAsync(Guid __leadId, FollowUpRequest __request, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__leadId, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __leadId);
        EnsureCanManageLead(lead);

        var followUp = new FollowUpModel
        {
            Id = Guid.NewGuid(),
            LeadId = __leadId,
            ScheduledAt = __request.ScheduledAt,
            Type = __request.Type,
            Notes = __request.Notes,
            Status = "Pending",
            CreatedBy = _currentUserAccessor.UserId
        };

        await _followUpRepository.CreateAsync(followUp, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "followup.created", "FollowUp", followUp.Id.ToString(), null, __request.Type, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "followup.created", "FollowUp", followUp.Id);
        }

        _logger.LogInformation("FollowUp {FollowUpId} created for {LeadId} by {UserId}", followUp.Id, __leadId, _currentUserAccessor.UserId);

        var created = await _followUpRepository.GetByIdAsync(followUp.Id, __cancellationToken) ?? followUp;
        return ToResponse(created);
    }

    public async Task<FollowUpResponse> UpdateStatusAsync(Guid __id, FollowUpStatusRequest __request, CancellationToken __cancellationToken)
    {
        var followUp = await _followUpRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("FollowUp", __id);
        var lead = await _leadRepository.GetByIdAsync(followUp.LeadId, __cancellationToken) ?? throw new EntityNotFoundException("Lead", followUp.LeadId);
        EnsureCanManageLead(lead);

        var oldStatus = followUp.Status;
        var completedAt = __request.Status == "Completed" ? DateTime.UtcNow : followUp.CompletedAt;

        await _followUpRepository.UpdateStatusAsync(__id, __request.Status, __request.Notes, completedAt, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "followup.status.changed", "FollowUp", __id.ToString(), oldStatus, __request.Status, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "followup.status.changed", "FollowUp", __id);
        }

        _logger.LogInformation("FollowUp {FollowUpId} status changed {OldStatus} -> {NewStatus} by {UserId}", __id, oldStatus, __request.Status, _currentUserAccessor.UserId);

        var updated = await _followUpRepository.GetByIdAsync(__id, __cancellationToken) ?? followUp;
        return ToResponse(updated);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var followUp = await _followUpRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("FollowUp", __id);
        var lead = await _leadRepository.GetByIdAsync(followUp.LeadId, __cancellationToken) ?? throw new EntityNotFoundException("Lead", followUp.LeadId);
        EnsureCanManageLead(lead);

        await _followUpRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "followup.deleted", "FollowUp", __id.ToString(), followUp.Type, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "followup.deleted", "FollowUp", __id);
        }

        _logger.LogInformation("FollowUp {FollowUpId} deleted by {UserId}", __id, _currentUserAccessor.UserId);
    }

    /// <summary>Mirrors LeadAppFunction.EnsureCanManage -- a follow-up's real owner is its parent
    /// lead's assigned agent, so the same "SuperAdmin bypasses, everyone else must own it" rule
    /// applies here through the lead rather than a separate, follow-up-specific ownership concept.</summary>
    private void EnsureCanManageLead(LeadModel __lead)
    {
        if (_currentUserAccessor.IsSuperAdmin) return;
        if (__lead.AssignedToUserId != _currentUserAccessor.UserId)
        {
            throw new ForbiddenException("This lead is assigned to another team member. Only that agent or an administrator can manage its follow-ups.");
        }
    }

    private static FollowUpResponse ToResponse(FollowUpModel followUp) => new()
    {
        Id = followUp.Id,
        LeadId = followUp.LeadId,
        LeadCustomerName = followUp.LeadCustomerName,
        ScheduledAt = followUp.ScheduledAt,
        Type = followUp.Type,
        Notes = followUp.Notes,
        Status = followUp.Status,
        CompletedAt = followUp.CompletedAt
    };
}
