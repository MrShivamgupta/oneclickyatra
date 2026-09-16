namespace OneClickYatra.Api.Models.Requests;

public sealed class DashboardDateRangeRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
