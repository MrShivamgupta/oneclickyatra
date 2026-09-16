using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IDashboardAppFunction
{
    Task<DashboardKpisResponse> GetKpisAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<RevenueTrendPointResponse>> GetRevenueTrendAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<LeadFunnelStageResponse>> GetLeadFunnelAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<DestinationPerformanceResponse>> GetDestinationPerformanceAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<SalesPerformanceResponse>> GetSalesPerformanceAsync(DashboardDateRangeRequest __request, CancellationToken __cancellationToken);

    Task<List<LeadResponse>> GetRecentLeadsAsync(CancellationToken __cancellationToken);
    Task<List<BookingResponse>> GetRecentBookingsAsync(CancellationToken __cancellationToken);
    Task<List<EnquiryResponse>> GetPendingEnquiriesAsync(CancellationToken __cancellationToken);
    Task<List<BookingResponse>> GetRefundRequestsAsync(CancellationToken __cancellationToken);
    Task<List<BookingResponse>> GetUpcomingDeparturesAsync(CancellationToken __cancellationToken);

    Task<List<DashboardAlertResponse>> GetAlertsAsync(CancellationToken __cancellationToken);
}
