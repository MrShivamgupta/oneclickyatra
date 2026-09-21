using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IFollowUpRepository
{
    Task<FollowUpModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<IReadOnlyList<FollowUpModel>> ListByLeadAsync(Guid __leadId, CancellationToken __cancellationToken);
    Task<PaginationResponse<FollowUpModel>> ListTodayAsync(PaginationRequest __request, CancellationToken __cancellationToken);

    /// <summary>Every non-deleted, still-Pending follow-up whose ScheduledAt has passed, across all
    /// leads (oldest first). Mirrors the same "overdue" predicate DashboardRepository.GetAlertsAsync
    /// already uses for its follow-up alert count.</summary>
    Task<IReadOnlyList<FollowUpModel>> ListOverdueAsync(CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(FollowUpModel __followUp, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, string? __notes, DateTime? __completedAt, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
