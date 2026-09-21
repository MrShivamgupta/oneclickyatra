using System.Data;
using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReportRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<IReadOnlyList<SalesReportRowModel>> GetSalesAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_Sales",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<SalesReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<RevenueReportRowModel>> GetRevenueAsync(DateOnly __fromDate, DateOnly __toDate, string __groupBy, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_Revenue",
            new { FromDate = __fromDate, ToDate = __toDate, GroupBy = __groupBy },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<RevenueReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<CancellationReportRowModel>> GetCancellationsAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_Cancellation",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<CancellationReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<AgentCommissionReportRowModel>> GetAgentCommissionAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_AgentCommission",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<AgentCommissionReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<LeadConversionReportRowModel>> GetLeadConversionAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_LeadConversion",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<LeadConversionReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<DestinationSalesReportRowModel>> GetDestinationSalesAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_DestinationSales",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<DestinationSalesReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<CollectionReportRowModel>> GetCollectionAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_Collection",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<CollectionReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<OutstandingReportRowModel>> GetOutstandingAsync(CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_Outstanding",
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<OutstandingReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<ProfitabilityReportRowModel>> GetProfitabilityAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_Profitability",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<ProfitabilityReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<ActiveBookingReportRowModel>> GetActiveBookingsAsync(CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_ActiveBookings",
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<ActiveBookingReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<EmployeeProductivityReportRowModel>> GetEmployeeProductivityAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_EmployeeProductivity",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<EmployeeProductivityReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<MonthlyGrowthReportRowModel>> GetMonthlyGrowthAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_MonthlyGrowth",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<MonthlyGrowthReportRowModel>(command);
        return result.ToList();
    }

    public async Task<IReadOnlyList<VendorPerformanceReportRowModel>> GetVendorPerformanceAsync(DateOnly __fromDate, DateOnly __toDate, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "sp_Report_VendorPerformance",
            new { FromDate = __fromDate, ToDate = __toDate },
            commandType: CommandType.StoredProcedure,
            cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<VendorPerformanceReportRowModel>(command);
        return result.ToList();
    }
}
