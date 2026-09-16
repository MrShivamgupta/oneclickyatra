namespace OneClickYatra.Api.Models;

/// <summary>Maps 1:1 to sp_Dashboard_GetKpis' single result row.</summary>
public sealed class DashboardKpisModel
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
}

public sealed class RevenueTrendPointModel
{
    public DateOnly TrendDate { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class LeadFunnelStageModel
{
    public string Status { get; set; } = string.Empty;
    public int LeadCount { get; set; }
}

public sealed class DestinationPerformanceModel
{
    public string DestinationName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class SalesPerformanceModel
{
    public string StaffName { get; set; } = string.Empty;
    public int LeadsHandled { get; set; }
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class DashboardAlertModel
{
    public string Severity { get; set; } = "info";
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
}
