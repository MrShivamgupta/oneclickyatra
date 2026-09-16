using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// Composes real, already-built domain data for the admin dashboard. The heavy aggregations
/// (KPIs, revenue trend, lead funnel, destination/sales performance) go through
/// <see cref="IDashboardRepository"/>'s stored procedures per §8; the "recent/pending/upcoming"
/// list widgets reuse the existing, already-tested AppFunctions for those domains instead of
/// duplicating their search logic here.
/// </summary>
public sealed class DashboardAppFunction : IDashboardAppFunction
{
    private const int WidgetRowLimit = 8;
    private const int UpcomingDepartureWindowDays = 30;

    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILeadAppFunction _leadAppFunction;
    private readonly IBookingAppFunction _bookingAppFunction;
    private readonly IEnquiryAppFunction _enquiryAppFunction;
    private readonly IFollowUpAppFunction _followUpAppFunction;

    public DashboardAppFunction(
        IDashboardRepository __dashboardRepository,
        ILeadAppFunction __leadAppFunction,
        IBookingAppFunction __bookingAppFunction,
        IEnquiryAppFunction __enquiryAppFunction,
        IFollowUpAppFunction __followUpAppFunction)
    {
        _dashboardRepository = __dashboardRepository;
        _leadAppFunction = __leadAppFunction;
        _bookingAppFunction = __bookingAppFunction;
        _enquiryAppFunction = __enquiryAppFunction;
        _followUpAppFunction = __followUpAppFunction;
    }

    public async Task<DashboardKpisResponse> GetKpisAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request);
        var kpis = await _dashboardRepository.GetKpisAsync(from, to, __cancellationToken);
        var todayFollowUps = await _followUpAppFunction.ListTodayAsync(new PaginationRequest { PageNumber = 1, PageSize = 1 }, __cancellationToken);

        return new DashboardKpisResponse
        {
            TotalLeads = kpis.TotalLeads,
            TotalBookings = kpis.TotalBookings,
            ConfirmedBookings = kpis.ConfirmedBookings,
            Cancellations = kpis.Cancellations,
            ActiveQuotations = kpis.ActiveQuotations,
            PeriodRevenue = kpis.PeriodRevenue,
            PendingPayments = kpis.PendingPayments,
            UpcomingDepartures = kpis.UpcomingDepartures,
            OpenEnquiries = kpis.OpenEnquiries,
            ConversionRate = kpis.ConversionRate,
            TodayFollowUps = (int)todayFollowUps.TotalCount,
            Range = new DashboardDateRangeRequest { FromDate = from, ToDate = to }
        };
    }

    public async Task<List<RevenueTrendPointResponse>> GetRevenueTrendAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request);
        var points = await _dashboardRepository.GetRevenueTrendAsync(from, to, __cancellationToken);
        return points.Select(p => new RevenueTrendPointResponse { TrendDate = p.TrendDate, Revenue = p.Revenue }).ToList();
    }

    public async Task<List<LeadFunnelStageResponse>> GetLeadFunnelAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request);
        var stages = await _dashboardRepository.GetLeadFunnelAsync(from, to, __cancellationToken);
        return stages.Select(s => new LeadFunnelStageResponse { Status = s.Status, LeadCount = s.LeadCount }).ToList();
    }

    public async Task<List<DestinationPerformanceResponse>> GetDestinationPerformanceAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request);
        var rows = await _dashboardRepository.GetDestinationPerformanceAsync(from, to, WidgetRowLimit, __cancellationToken);
        return rows.Select(r => new DestinationPerformanceResponse { DestinationName = r.DestinationName, BookingCount = r.BookingCount, Revenue = r.Revenue }).ToList();
    }

    public async Task<List<SalesPerformanceResponse>> GetSalesPerformanceAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var (from, to) = NormalizeRange(__request);
        var rows = await _dashboardRepository.GetSalesPerformanceAsync(from, to, WidgetRowLimit, __cancellationToken);
        return rows.Select(r => new SalesPerformanceResponse { StaffName = r.StaffName, LeadsHandled = r.LeadsHandled, BookingsCount = r.BookingsCount, Revenue = r.Revenue }).ToList();
    }

    public async Task<List<LeadResponse>> GetRecentLeadsAsync(CancellationToken __cancellationToken)
    {
        var page = await _leadAppFunction.SearchAsync(new LeadSearchRequest { PageNumber = 1, PageSize = WidgetRowLimit }, __cancellationToken);
        return page.Items.ToList();
    }

    public async Task<List<BookingResponse>> GetRecentBookingsAsync(CancellationToken __cancellationToken)
    {
        var page = await _bookingAppFunction.SearchAsync(new BookingSearchRequest { PageNumber = 1, PageSize = WidgetRowLimit }, __cancellationToken);
        return page.Items.ToList();
    }

    public async Task<List<EnquiryResponse>> GetPendingEnquiriesAsync(CancellationToken __cancellationToken)
    {
        var page = await _enquiryAppFunction.SearchAsync(new EnquirySearchRequest { Status = "New", PageNumber = 1, PageSize = WidgetRowLimit }, __cancellationToken);
        return page.Items.ToList();
    }

    public async Task<List<BookingResponse>> GetRefundRequestsAsync(CancellationToken __cancellationToken)
    {
        var page = await _bookingAppFunction.SearchAsync(new BookingSearchRequest { Status = "RefundPending", PageNumber = 1, PageSize = WidgetRowLimit }, __cancellationToken);
        return page.Items.ToList();
    }

    public Task<List<BookingResponse>> GetUpcomingDeparturesAsync(CancellationToken __cancellationToken)
        => _bookingAppFunction.GetUpcomingDeparturesAsync(UpcomingDepartureWindowDays, WidgetRowLimit, __cancellationToken);

    public async Task<List<DashboardAlertResponse>> GetAlertsAsync(CancellationToken __cancellationToken)
    {
        var alerts = await _dashboardRepository.GetAlertsAsync(__cancellationToken);
        return alerts.Select(a => new DashboardAlertResponse { Severity = a.Severity, Message = a.Message, Link = a.Link }).ToList();
    }

    private static (DateOnly From, DateOnly To) NormalizeRange(DashboardDateRangeRequest __request)
    {
        var to = __request.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = __request.FromDate ?? to.AddDays(-30);
        return (from, to);
    }
}
