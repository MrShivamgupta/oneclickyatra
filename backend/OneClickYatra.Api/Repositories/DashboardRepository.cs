using System.Data;
using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DashboardRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<DashboardKpisModel> GetKpisAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Dashboard_GetKpis",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        return await connection.QuerySingleAsync<DashboardKpisModel>(command);
    }

    public async Task<IReadOnlyList<RevenueTrendPointModel>> GetRevenueTrendAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Dashboard_GetRevenueTrend",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<RevenueTrendPointModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<LeadFunnelStageModel>> GetLeadFunnelAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Dashboard_GetLeadFunnel",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<LeadFunnelStageModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<DestinationPerformanceModel>> GetDestinationPerformanceAsync(DateOnly __fromDate, DateOnly __toDate, int __top, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Dashboard_GetDestinationPerformance",
            new { FromDate = __fromDate, ToDate = __toDate, Top = __top },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<DestinationPerformanceModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<SalesPerformanceModel>> GetSalesPerformanceAsync(DateOnly __fromDate, DateOnly __toDate, int __top, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Dashboard_GetSalesPerformance",
            new { FromDate = __fromDate, ToDate = __toDate, Top = __top },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<SalesPerformanceModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<DashboardAlertModel>> GetAlertsAsync(CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*) FROM FollowUps WHERE IsDeleted = 0 AND Status = 'Pending' AND ScheduledAt < SYSUTCDATETIME();
            SELECT COUNT(*) FROM Quotations WHERE IsDeleted = 0 AND Status = 'Sent' AND ValidUntil IS NOT NULL
                AND ValidUntil <= DATEADD(DAY, 3, CAST(SYSUTCDATETIME() AS DATE));
            SELECT COUNT(*) FROM Bookings WHERE IsDeleted = 0 AND Status = 'PendingPayment' AND CreatedAt < DATEADD(DAY, -3, SYSUTCDATETIME());
            """;

        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(new CommandDefinition(sql, cancellationToken: __cancellationToken));

        var overdueFollowUps = await multi.ReadSingleAsync<int>();
        var expiringQuotations = await multi.ReadSingleAsync<int>();
        var stuckPayments = await multi.ReadSingleAsync<int>();

        var alerts = new List<DashboardAlertModel>();
        if (overdueFollowUps > 0)
        {
            alerts.Add(new DashboardAlertModel { Severity = "warning", Message = $"{overdueFollowUps} follow-up(s) are overdue.", Link = "/admin/followups" });
        }
        if (expiringQuotations > 0)
        {
            alerts.Add(new DashboardAlertModel { Severity = "warning", Message = $"{expiringQuotations} quotation(s) expire within 3 days.", Link = "/admin/quotations" });
        }
        if (stuckPayments > 0)
        {
            alerts.Add(new DashboardAlertModel { Severity = "danger", Message = $"{stuckPayments} booking(s) have been awaiting payment for over 3 days.", Link = "/admin/bookings" });
        }

        return alerts;
    }
}
