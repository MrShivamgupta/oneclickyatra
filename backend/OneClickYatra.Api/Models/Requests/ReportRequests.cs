namespace OneClickYatra.Api.Models.Requests;

/// <summary>Shared date-range request for the reports that scope by a from/to date. Mirrors
/// DashboardDateRangeRequest exactly, including the default-to-last-30-days normalization done in
/// ReportAppFunction, not the controller.</summary>
public sealed class ReportDateRangeRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

/// <summary>Used by the Revenue and Monthly Growth reports, which both trend over time and can be
/// grouped by period. GroupBy is one of "day"/"week"/"month" (Monthly Growth ignores it — it is
/// always grouped by calendar month — but shares this request shape rather than adding a second,
/// near-identical DTO).</summary>
public sealed class ReportGroupedDateRangeRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? GroupBy { get; set; }
}
