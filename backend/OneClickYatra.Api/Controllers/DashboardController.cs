using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[HasPermission(PermissionConstants.DashboardView)]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardAppFunction _dashboardAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public DashboardController(IDashboardAppFunction __dashboardAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _dashboardAppFunction = __dashboardAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardKpisResponse>>> Stats([FromQuery] DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetKpisAsync(__request, __cancellationToken);
        return Ok(ApiResponse<DashboardKpisResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("revenue-chart")]
    public async Task<ActionResult<ApiResponse<List<RevenueTrendPointResponse>>>> RevenueChart([FromQuery] DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetRevenueTrendAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<RevenueTrendPointResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("lead-funnel")]
    public async Task<ActionResult<ApiResponse<List<LeadFunnelStageResponse>>>> LeadFunnel([FromQuery] DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetLeadFunnelAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<LeadFunnelStageResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("destination-performance")]
    public async Task<ActionResult<ApiResponse<List<DestinationPerformanceResponse>>>> DestinationPerformance([FromQuery] DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetDestinationPerformanceAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<DestinationPerformanceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("sales-performance")]
    public async Task<ActionResult<ApiResponse<List<SalesPerformanceResponse>>>> SalesPerformance([FromQuery] DashboardDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetSalesPerformanceAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<SalesPerformanceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("recent-leads")]
    public async Task<ActionResult<ApiResponse<List<LeadResponse>>>> RecentLeads(CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetRecentLeadsAsync(__cancellationToken);
        return Ok(ApiResponse<List<LeadResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("recent-bookings")]
    public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> RecentBookings(CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetRecentBookingsAsync(__cancellationToken);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("pending-enquiries")]
    public async Task<ActionResult<ApiResponse<List<EnquiryResponse>>>> PendingEnquiries(CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetPendingEnquiriesAsync(__cancellationToken);
        return Ok(ApiResponse<List<EnquiryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("refund-requests")]
    public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> RefundRequests(CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetRefundRequestsAsync(__cancellationToken);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("upcoming-departures")]
    public async Task<ActionResult<ApiResponse<List<BookingResponse>>>> UpcomingDepartures(CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetUpcomingDeparturesAsync(__cancellationToken);
        return Ok(ApiResponse<List<BookingResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<ApiResponse<List<DashboardAlertResponse>>>> Alerts(CancellationToken __cancellationToken)
    {
        var result = await _dashboardAppFunction.GetAlertsAsync(__cancellationToken);
        return Ok(ApiResponse<List<DashboardAlertResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }
}
