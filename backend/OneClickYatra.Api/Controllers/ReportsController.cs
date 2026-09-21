using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[HasPermission(PermissionConstants.ReportView)]
public sealed class ReportsController : ControllerBase
{
    private const string CsvContentType = "text/csv";
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    private readonly IReportAppFunction _reportAppFunction;
    private readonly IReportExportService _reportExportService;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public ReportsController(IReportAppFunction __reportAppFunction, IReportExportService __reportExportService, ITrackingIdAccessor __trackingIdAccessor)
    {
        _reportAppFunction = __reportAppFunction;
        _reportExportService = __reportExportService;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<List<SalesReportResponse>>>> Sales([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetSalesAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<SalesReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("sales/export")]
    public async Task<IActionResult> SalesExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetSalesAsync(__request, __cancellationToken);
        return Export(rows, "Sales", format);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<List<RevenueReportResponse>>>> Revenue([FromQuery] ReportGroupedDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetRevenueAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<RevenueReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("revenue/export")]
    public async Task<IActionResult> RevenueExport([FromQuery] ReportGroupedDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetRevenueAsync(__request, __cancellationToken);
        return Export(rows, "Revenue", format);
    }

    [HttpGet("cancellation")]
    public async Task<ActionResult<ApiResponse<List<CancellationReportResponse>>>> Cancellation([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetCancellationsAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<CancellationReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("cancellation/export")]
    public async Task<IActionResult> CancellationExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetCancellationsAsync(__request, __cancellationToken);
        return Export(rows, "Cancellation", format);
    }

    [HttpGet("agent-commission")]
    public async Task<ActionResult<ApiResponse<List<AgentCommissionReportResponse>>>> AgentCommission([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetAgentCommissionAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<AgentCommissionReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("agent-commission/export")]
    public async Task<IActionResult> AgentCommissionExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetAgentCommissionAsync(__request, __cancellationToken);
        return Export(rows, "AgentCommission", format);
    }

    [HttpGet("lead-conversion")]
    public async Task<ActionResult<ApiResponse<List<LeadConversionReportResponse>>>> LeadConversion([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetLeadConversionAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<LeadConversionReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("lead-conversion/export")]
    public async Task<IActionResult> LeadConversionExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetLeadConversionAsync(__request, __cancellationToken);
        return Export(rows, "LeadConversion", format);
    }

    [HttpGet("destination-sales")]
    public async Task<ActionResult<ApiResponse<List<DestinationSalesReportResponse>>>> DestinationSales([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetDestinationSalesAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<DestinationSalesReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("destination-sales/export")]
    public async Task<IActionResult> DestinationSalesExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetDestinationSalesAsync(__request, __cancellationToken);
        return Export(rows, "DestinationSales", format);
    }

    [HttpGet("collection")]
    public async Task<ActionResult<ApiResponse<List<CollectionReportResponse>>>> Collection([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetCollectionAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<CollectionReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("collection/export")]
    public async Task<IActionResult> CollectionExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetCollectionAsync(__request, __cancellationToken);
        return Export(rows, "Collection", format);
    }

    [HttpGet("outstanding")]
    public async Task<ActionResult<ApiResponse<List<OutstandingReportResponse>>>> Outstanding(CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetOutstandingAsync(__cancellationToken);
        return Ok(ApiResponse<List<OutstandingReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("outstanding/export")]
    public async Task<IActionResult> OutstandingExport([FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetOutstandingAsync(__cancellationToken);
        return Export(rows, "Outstanding", format);
    }

    [HttpGet("profitability")]
    public async Task<ActionResult<ApiResponse<List<ProfitabilityReportResponse>>>> Profitability([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetProfitabilityAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<ProfitabilityReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("profitability/export")]
    public async Task<IActionResult> ProfitabilityExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetProfitabilityAsync(__request, __cancellationToken);
        return Export(rows, "Profitability", format);
    }

    [HttpGet("active-bookings")]
    public async Task<ActionResult<ApiResponse<List<ActiveBookingReportResponse>>>> ActiveBookings(CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetActiveBookingsAsync(__cancellationToken);
        return Ok(ApiResponse<List<ActiveBookingReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("active-bookings/export")]
    public async Task<IActionResult> ActiveBookingsExport([FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetActiveBookingsAsync(__cancellationToken);
        return Export(rows, "ActiveBookings", format);
    }

    [HttpGet("employee-productivity")]
    public async Task<ActionResult<ApiResponse<List<EmployeeProductivityReportResponse>>>> EmployeeProductivity([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetEmployeeProductivityAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<EmployeeProductivityReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("employee-productivity/export")]
    public async Task<IActionResult> EmployeeProductivityExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetEmployeeProductivityAsync(__request, __cancellationToken);
        return Export(rows, "EmployeeProductivity", format);
    }

    [HttpGet("monthly-growth")]
    public async Task<ActionResult<ApiResponse<List<MonthlyGrowthReportResponse>>>> MonthlyGrowth([FromQuery] ReportGroupedDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetMonthlyGrowthAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<MonthlyGrowthReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("monthly-growth/export")]
    public async Task<IActionResult> MonthlyGrowthExport([FromQuery] ReportGroupedDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetMonthlyGrowthAsync(__request, __cancellationToken);
        return Export(rows, "MonthlyGrowth", format);
    }

    [HttpGet("vendor-performance")]
    public async Task<ActionResult<ApiResponse<List<VendorPerformanceReportResponse>>>> VendorPerformance([FromQuery] ReportDateRangeRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _reportAppFunction.GetVendorPerformanceAsync(__request, __cancellationToken);
        return Ok(ApiResponse<List<VendorPerformanceReportResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("vendor-performance/export")]
    public async Task<IActionResult> VendorPerformanceExport([FromQuery] ReportDateRangeRequest __request, [FromQuery] string format, CancellationToken __cancellationToken)
    {
        var rows = await _reportAppFunction.GetVendorPerformanceAsync(__request, __cancellationToken);
        return Export(rows, "VendorPerformance", format);
    }

    /// <summary>Shared by every export action: pipes rows through the requested format and returns
    /// the file with the right content type. Defaults to CSV for an unrecognized/missing format.</summary>
    private IActionResult Export<T>(List<T> __rows, string __reportName, string? __format)
    {
        return (__format ?? string.Empty).ToLowerInvariant() switch
        {
            "xlsx" => File(_reportExportService.ToXlsx(__rows, __reportName), XlsxContentType, $"{__reportName}.xlsx"),
            "pdf" => File(_reportExportService.ToPdf($"{__reportName} Report", __rows), PdfContentType, $"{__reportName}.pdf"),
            _ => File(_reportExportService.ToCsv(__rows), CsvContentType, $"{__reportName}.csv")
        };
    }
}
