using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IReportAppFunction
{
    Task<List<SalesReportResponse>> GetSalesAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<RevenueReportResponse>> GetRevenueAsync(ReportGroupedDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<CancellationReportResponse>> GetCancellationsAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<AgentCommissionReportResponse>> GetAgentCommissionAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<LeadConversionReportResponse>> GetLeadConversionAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<DestinationSalesReportResponse>> GetDestinationSalesAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<CollectionReportResponse>> GetCollectionAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<OutstandingReportResponse>> GetOutstandingAsync(CancellationToken __cancellationToken);
    Task<List<ProfitabilityReportResponse>> GetProfitabilityAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<ActiveBookingReportResponse>> GetActiveBookingsAsync(CancellationToken __cancellationToken);
    Task<List<EmployeeProductivityReportResponse>> GetEmployeeProductivityAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<MonthlyGrowthReportResponse>> GetMonthlyGrowthAsync(ReportGroupedDateRangeRequest __request, CancellationToken __cancellationToken);
    Task<List<VendorPerformanceReportResponse>> GetVendorPerformanceAsync(ReportDateRangeRequest __request, CancellationToken __cancellationToken);
}
