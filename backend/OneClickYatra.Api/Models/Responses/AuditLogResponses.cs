namespace OneClickYatra.Api.Models.Responses;

/// <summary>Read projection of an AuditLogs row for the admin audit-log viewer. ActorUserId/ActorName
/// are null for system actions with no acting user (AuditLogs.UserId is nullable) — ActorName
/// combines the acting user's name and email (from the LEFT JOIN to Users) into one display string.</summary>
public sealed class AuditLogResponse
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
