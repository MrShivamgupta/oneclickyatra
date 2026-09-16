namespace OneClickYatra.Api.Models.Interfaces;

public interface IFollowUpModel
{
    Guid Id { get; }
    Guid LeadId { get; }
    DateTime ScheduledAt { get; }
    string Type { get; }
    string? Notes { get; }
    string Status { get; }
    DateTime? CompletedAt { get; }
    bool IsDeleted { get; }
}
