using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

/// <summary>Filters for the admin audit-log viewer. All filters are optional and combine with AND.
/// FromDate/ToDate scope against AuditLogs.CreatedAt and follow the same DateOnly plus
/// less-than-next-day convention as FeedbackSearchRequest, since CreatedAt is a DATETIME2
/// (timestamp, not just a date).</summary>
public sealed class AuditLogSearchRequest : PaginationRequest
{
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
