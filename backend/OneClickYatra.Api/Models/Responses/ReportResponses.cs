namespace OneClickYatra.Api.Models.Responses;

public sealed class SalesReportResponse
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? DestinationName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AssignedAgentName { get; set; }
}

public sealed class RevenueReportResponse
{
    public DateOnly PeriodStart { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class CancellationReportResponse
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>No AgentCommissions/rate-config table exists yet — see ReportAppFunction's summary
/// note. Revenue/BookingsCount only, never a fabricated commission amount.</summary>
public sealed class AgentCommissionReportResponse
{
    public string StaffName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class LeadConversionReportResponse
{
    public string Source { get; set; } = string.Empty;
    public int TotalLeads { get; set; }
    public int ConvertedLeads { get; set; }
}

public sealed class DestinationSalesReportResponse
{
    public string DestinationName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class CollectionReportResponse
{
    public Guid PaymentReference { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OutstandingReportResponse
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
}

/// <summary>Revenue-only — this schema has no cost/expense tracking table, so a true profit
/// margin cannot be computed. Never rename this to "Profit"; it is not profit.</summary>
public sealed class ProfitabilityReportResponse
{
    public string DestinationName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public sealed class ActiveBookingReportResponse
{
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateOnly? TravelDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class EmployeeProductivityReportResponse
{
    public string StaffName { get; set; } = string.Empty;
    public int LeadsAssigned { get; set; }
    public int FollowUpsCompleted { get; set; }
    public int QuotationsSent { get; set; }
}

public sealed class MonthlyGrowthReportResponse
{
    public DateOnly MonthStart { get; set; }
    public int NewLeads { get; set; }
    public int NewBookings { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>The 12th SRS report, added once both the Vendor and Reports domains had landed.</summary>
public sealed class VendorPerformanceReportResponse
{
    public string VendorName { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public int RatingsCount { get; set; }
    public decimal? AverageRating { get; set; }
    public int PaymentsCount { get; set; }
    public decimal TotalPaid { get; set; }
}
