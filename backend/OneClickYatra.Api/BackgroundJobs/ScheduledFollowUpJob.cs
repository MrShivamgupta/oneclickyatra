using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>Recurring Hangfire job: surfaces follow-ups that are overdue (ScheduledAt has passed and
/// the follow-up is still Pending) so operations staff have a signal in the log stream. No
/// email/notification channel was specified for this in the SRS beyond "scheduled", so a structured
/// log signal — one Warning per overdue follow-up, plus a summary count — is the full deliverable
/// for this pass.</summary>
public sealed class ScheduledFollowUpJob
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly ILogger<ScheduledFollowUpJob> _logger;

    public ScheduledFollowUpJob(IFollowUpRepository __followUpRepository, ILogger<ScheduledFollowUpJob> __logger)
    {
        _followUpRepository = __followUpRepository;
        _logger = __logger;
    }

    public async Task RunAsync(CancellationToken __cancellationToken = default)
    {
        var overdueFollowUps = await _followUpRepository.ListOverdueAsync(__cancellationToken);

        foreach (var followUp in overdueFollowUps)
        {
            _logger.LogWarning(
                "ScheduledFollowUpJob: follow-up {FollowUpId} for lead {LeadId} ({LeadCustomerName}) is overdue. ScheduledAt: {ScheduledAt:o}, Type: {Type}.",
                followUp.Id, followUp.LeadId, followUp.LeadCustomerName, followUp.ScheduledAt, followUp.Type);
        }

        _logger.LogInformation("ScheduledFollowUpJob completed. OverdueCount: {OverdueCount}.", overdueFollowUps.Count);
    }
}
