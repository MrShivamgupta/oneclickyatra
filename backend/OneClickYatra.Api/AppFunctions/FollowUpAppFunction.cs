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

    public FollowUpAppFunction(
        IFollowUpRepository __followUpRepository,
        ILeadRepository __leadRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter)
    {
        _followUpRepository = __followUpRepository;
        _leadRepository = __leadRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
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
        _ = await _leadRepository.GetByIdAsync(__leadId, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __leadId);

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
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "followup.created", "FollowUp", followUp.Id.ToString(), null, __request.Type, __cancellationToken);

        var created = await _followUpRepository.GetByIdAsync(followUp.Id, __cancellationToken) ?? followUp;
        return ToResponse(created);
    }

    public async Task<FollowUpResponse> UpdateStatusAsync(Guid __id, FollowUpStatusRequest __request, CancellationToken __cancellationToken)
    {
        var followUp = await _followUpRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("FollowUp", __id);

        var oldStatus = followUp.Status;
        var completedAt = __request.Status == "Completed" ? DateTime.UtcNow : followUp.CompletedAt;

        await _followUpRepository.UpdateStatusAsync(__id, __request.Status, __request.Notes, completedAt, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "followup.status.changed", "FollowUp", __id.ToString(), oldStatus, __request.Status, __cancellationToken);

        var updated = await _followUpRepository.GetByIdAsync(__id, __cancellationToken) ?? followUp;
        return ToResponse(updated);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var followUp = await _followUpRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("FollowUp", __id);

        await _followUpRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "followup.deleted", "FollowUp", __id.ToString(), followUp.Type, null, __cancellationToken);
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
