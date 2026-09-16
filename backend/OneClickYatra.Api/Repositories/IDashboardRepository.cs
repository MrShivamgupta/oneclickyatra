using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IDashboardRepository
{
    Task<DashboardKpisModel> GetKpisAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<RevenueTrendPointModel>> GetRevenueTrendAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<LeadFunnelStageModel>> GetLeadFunnelAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken);
    Task<IReadOnlyList<DestinationPerformanceModel>> GetDestinationPerformanceAsync(DateOnly __fromDate, DateOnly __toDate, int __top, CancellationToken __cancellationToken);
    Task<IReadOnlyList<SalesPerformanceModel>> GetSalesPerformanceAsync(DateOnly __fromDate, DateOnly __toDate, int __top, CancellationToken __cancellationToken);
    Task<IReadOnlyList<DashboardAlertModel>> GetAlertsAsync(CancellationToken __cancellationToken);
}
