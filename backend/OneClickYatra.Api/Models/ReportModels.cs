namespace OneClickYatra.Api.Models;

/// <summary>Maps 1:1 to sp_Report_Sales' result rows.</summary>
public sealed class SalesReportRowModel
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? DestinationName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AssignedAgentName { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_Revenue's result rows.</summary>
public sealed class RevenueReportRowModel
{
    public DateOnly PeriodStart { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_Cancellation's result rows.</summary>
public sealed class CancellationReportRowModel
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Maps 1:1 to sp_Report_AgentCommission's result rows. There is no AgentCommissions/rate-config
/// table in this schema yet, so this intentionally carries only StaffName/BookingsCount/Revenue —
/// never a fabricated commission percentage or amount.
/// </summary>
public sealed class AgentCommissionReportRowModel
{
    public string StaffName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_LeadConversion's result rows.</summary>
public sealed class LeadConversionReportRowModel
{
    public string Source { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_DestinationSales' result rows.</summary>
public sealed class DestinationSalesReportRowModel
{
    public string DestinationName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_Collection's result rows.</summary>
public sealed class CollectionReportRowModel
{
    public Guid PaymentReference { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_Outstanding's result rows (point-in-time snapshot, no date range).</summary>
public sealed class OutstandingReportRowModel
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
}

/// <summary>
/// Maps 1:1 to sp_Report_Profitability's result rows. Revenue-only — this schema has no
/// cost/expense tracking table, so a true profit margin cannot be computed. Never rename this to
/// "Profit"; it is not profit.
/// </summary>
public sealed class ProfitabilityReportRowModel
{
    public string DestinationName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_ActiveBookings' result rows (point-in-time snapshot, no date range).</summary>
public sealed class ActiveBookingReportRowModel
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly? TravelDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Maps 1:1 to sp_Report_EmployeeProductivity's result rows.</summary>
public sealed class EmployeeProductivityReportRowModel
{
    public string StaffName { get; set; } = string.Empty;
    public int LeadsAssigned { get; set; }
    public int FollowUpsCompleted { get; set; }
    public int QuotationsSent { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_MonthlyGrowth's result rows.</summary>
public sealed class MonthlyGrowthReportRowModel
{
    public DateOnly MonthStart { get; set; }
    public int NewLeads { get; set; }
    public int NewBookings { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>Maps 1:1 to sp_Report_VendorPerformance's result rows (the 12th SRS report, added
/// after both the Vendor and Reports domains had landed).</summary>
public sealed class VendorPerformanceReportRowModel
{
    public string VendorName { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public int RatingsCount { get; set; }
    public decimal? AverageRating { get; set; }
    public int PaymentsCount { get; set; }
    public decimal TotalPaid { get; set; }
}
