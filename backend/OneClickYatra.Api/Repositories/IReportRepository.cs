using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IReportRepository
{
    Task<IReadOnlyList<SalesReportRowModel>> GetSalesAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<RevenueReportRowModel>> GetRevenueAsync(DateOnly __fromDate, DateOnly __toDate, string __groupBy, CancellationToken __cancellationToken);
    Task<IReadOnlyList<CancellationReportRowModel>> GetCancellationsAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<AgentCommissionReportRowModel>> GetAgentCommissionAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<LeadConversionReportRowModel>> GetLeadConversionAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<DestinationSalesReportRowModel>> GetDestinationSalesAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<CollectionReportRowModel>> GetCollectionAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<OutstandingReportRowModel>> GetOutstandingAsync(CancellationToken __cancellationToken);
    Task<IReadOnlyList<ProfitabilityReportRowModel>> GetProfitabilityAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<ActiveBookingReportRowModel>> GetActiveBookingsAsync(CancellationToken __cancellationToken);
    Task<IReadOnlyList<EmployeeProductivityReportRowModel>> GetEmployeeProductivityAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<MonthlyGrowthReportRowModel>> GetMonthlyGrowthAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<VendorPerformanceReportRowModel>> GetVendorPerformanceAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
}
