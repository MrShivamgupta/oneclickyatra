using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// Thin pass-through over <see cref="IReportRepository"/>'s stored procedures — there is no
/// business-rule branching here, just the same date-range normalization DashboardAppFunction
/// established (default to the last 30 days when either end is omitted) plus a 1:1 row mapping.
///
/// All 12 SRS-named reports are implemented here. Vendor-Performance was added after Vendor
/// Management landed (it was built in a separate, concurrent slice and deliberately deferred until
/// the Vendor schema existed). AgentCommission and Profitability are revenue/count only: this
/// schema has no commission-rate config table and no cost/expense tracking table, so nothing here
/// fabricates a commission amount or a profit margin.
/// </summary>
public sealed class ReportAppFunction : IReportAppFunction
{
    private const string DefaultGroupBy = "day";
    private static readonly HashSet<string> AllowedGroupings = new(StringComparer.OrdinalIgnoreCase) { "day", "week", "month" };

    private readonly IReportRepository _reportRepository;

    public ReportAppFunction(IReportRepository __reportRepository)
    {
        _reportRepository = __reportRepository;
    }

    public async Task<List<SalesReportResponse>> GetSalesAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetSalesAsync(from, to, __cancellationToken);
        return rows.Select(r => new SalesReportResponse
        {
            BookingNumber = r.BookingNumber,
            CustomerName = r.CustomerName,
            DestinationName = r.DestinationName,
            TotalAmount = r.TotalAmount,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            AssignedAgentName = r.AssignedAgentName
        }).ToList();
    }

    public async Task<List<RevenueReportResponse>> GetRevenueAsync(ReportGroupedDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var groupBy = NormalizeGroupBy(__request.GroupBy);
        var rows = await _reportRepository.GetRevenueAsync(from, to, groupBy, __cancellationToken);
        return rows.Select(r => new RevenueReportResponse { PeriodStart = r.PeriodStart, Revenue = r.Revenue }).ToList();
    }

    public async Task<List<CancellationReportResponse>> GetCancellationsAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetCancellationsAsync(from, to, __cancellationToken);
        return rows.Select(r => new CancellationReportResponse
        {
            BookingNumber = r.BookingNumber,
            CustomerName = r.CustomerName,
            TotalAmount = r.TotalAmount,
            CancellationReason = r.CancellationReason,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<List<AgentCommissionReportResponse>> GetAgentCommissionAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetAgentCommissionAsync(from, to, __cancellationToken);
        return rows.Select(r => new AgentCommissionReportResponse { StaffName = r.StaffName, BookingsCount = r.BookingsCount, Revenue = r.Revenue }).ToList();
    }

    public async Task<List<LeadConversionReportResponse>> GetLeadConversionAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetLeadConversionAsync(from, to, __cancellationToken);
        return rows.Select(r => new LeadConversionReportResponse { Source = r.Source, TotalLeads = r.TotalLeads, ConvertedLeads = r.ConvertedLeads }).ToList();
    }

    public async Task<List<DestinationSalesReportResponse>> GetDestinationSalesAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetDestinationSalesAsync(from, to, __cancellationToken);
        return rows.Select(r => new DestinationSalesReportResponse { DestinationName = r.DestinationName, BookingCount = r.BookingCount, Revenue = r.Revenue }).ToList();
    }

    public async Task<List<CollectionReportResponse>> GetCollectionAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetCollectionAsync(from, to, __cancellationToken);
        return rows.Select(r => new CollectionReportResponse
        {
            PaymentReference = r.PaymentReference,
            BookingNumber = r.BookingNumber,
            CustomerName = r.CustomerName,
            Amount = r.Amount,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<List<OutstandingReportResponse>> GetOutstandingAsync(CancellationToken __cancellationToken)
    {
        var rows = await _reportRepository.GetOutstandingAsync(__cancellationToken);
        return rows.Select(r => new OutstandingReportResponse
        {
            BookingNumber = r.BookingNumber,
            CustomerName = r.CustomerName,
            TotalAmount = r.TotalAmount,
            AmountPaid = r.AmountPaid,
            OutstandingAmount = r.OutstandingAmount
        }).ToList();
    }

    public async Task<List<ProfitabilityReportResponse>> GetProfitabilityAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetProfitabilityAsync(from, to, __cancellationToken);
        return rows.Select(r => new ProfitabilityReportResponse { DestinationName = r.DestinationName, Revenue = r.Revenue }).ToList();
    }

    public async Task<List<ActiveBookingReportResponse>> GetActiveBookingsAsync(CancellationToken __cancellationToken)
    {
        var rows = await _reportRepository.GetActiveBookingsAsync(__cancellationToken);
        return rows.Select(r => new ActiveBookingReportResponse
        {
            BookingNumber = r.BookingNumber,
            CustomerName = r.CustomerName,
            TravelDate = r.TravelDate,
            Status = r.Status
        }).ToList();
    }

    public async Task<List<EmployeeProductivityReportResponse>> GetEmployeeProductivityAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetEmployeeProductivityAsync(from, to, __cancellationToken);
        return rows.Select(r => new EmployeeProductivityReportResponse
        {
            StaffName = r.StaffName,
            LeadsAssigned = r.LeadsAssigned,
            FollowUpsCompleted = r.FollowUpsCompleted,
            QuotationsSent = r.QuotationsSent
        }).ToList();
    }

    public async Task<List<MonthlyGrowthReportResponse>> GetMonthlyGrowthAsync(ReportGroupedDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetMonthlyGrowthAsync(from, to, __cancellationToken);
        return rows.Select(r => new MonthlyGrowthReportResponse
        {
            MonthStart = r.MonthStart,
            NewLeads = r.NewLeads,
            NewBookings = r.NewBookings,
            Revenue = r.Revenue
        }).ToList();
    }

    public async Task<List<VendorPerformanceReportResponse>> GetVendorPerformanceAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request.FromDate, __request.ToDate);
        var rows = await _reportRepository.GetVendorPerformanceAsync(from, to, __cancellationToken);
        return rows.Select(r => new VendorPerformanceReportResponse
        {
            VendorName = r.VendorName,
            VendorType = r.VendorType,
            RatingsCount = r.RatingsCount,
            AverageRating = r.AverageRating,
            PaymentsCount = r.PaymentsCount,
            TotalPaid = r.TotalPaid
        }).ToList();
    }

    private static (DateOnly From, DateOnly To) NormalizeRange(DateOnly? __fromDate, DateOnly? __toDate)
    {
        var to = __toDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = __fromDate ?? to.AddDays(-30);
        return (from, to);
    }

    private static string NormalizeGroupBy(string? __groupBy)
        => __groupBy is not null && AllowedGroupings.Contains(__groupBy) ? __groupBy.ToLowerInvariant() : DefaultGroupBy;
}
