namespace OneClickYatra.Api.Models;

public sealed class AuditLogModel
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    /// <summary>Populated only by AuditLogRepository.SearchAsync's LEFT JOIN to Users (null for
    /// system actions with no acting user, or when UserId's account was hard-deleted). Ignored by
    /// CreateAsync's INSERT — Dapper only binds parameters actually referenced by that SQL text.</summary>
    public string? ActorName { get; set; }
    public string? ActorEmail { get; set; }
}
