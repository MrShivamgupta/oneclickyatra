using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Models.Responses;

public sealed class DashboardKpisResponse
{
    public int TotalLeads { get; set; }
    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int Cancellations { get; set; }
    public int ActiveQuotations { get; set; }
    public decimal PeriodRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public int UpcomingDepartures { get; set; }
    public int OpenEnquiries { get; set; }
    public decimal ConversionRate { get; set; }
    public int TodayFollowUps { get; set; }
    public DashboardDateRangeRequest Range { get; set; } = new();
}

public sealed class RevenueTrendPointResponse
{
    public DateOnly TrendDate { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class LeadFunnelStageResponse
{
    public string Status { get; set; } = string.Empty;
    public int LeadCount { get; set; }
}

public sealed class DestinationPerformanceResponse
{
    public string DestinationName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class SalesPerformanceResponse
{
    public string StaffName { get; set; } = string.Empty;
    public int LeadsHandled { get; set; }
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class DashboardAlertResponse
{
    public string Severity { get; set; } = "info";
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
}
