using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private const string SelectColumns = """
        SELECT a.Id, a.UserId, a.Action, a.EntityName, a.EntityId, a.OldValue, a.NewValue,
               a.IpAddress, a.UserAgent, a.TrackingId, a.CreatedAt,
               u.FullName AS ActorName, u.Email AS ActorEmail
        FROM AuditLogs a
        LEFT JOIN Users u ON u.Id = a.UserId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public AuditLogRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task CreateAsync(AuditLogModel __entry, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO AuditLogs (Id, UserId, Action, EntityName, EntityId, OldValue, NewValue, IpAddress, UserAgent, TrackingId, CreatedAt)
            VALUES (@Id, @UserId, @Action, @EntityName, @EntityId, @OldValue, @NewValue, @IpAddress, @UserAgent, @TrackingId, SYSUTCDATETIME())
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, __entry, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<PaginationResponse<AuditLogModel>> SearchAsync(AuditLogSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE (@UserId IS NULL OR a.UserId = @UserId)
              AND (@Action IS NULL OR a.Action LIKE '%' + @Action + '%')
              AND (@EntityName IS NULL OR a.EntityName = @EntityName)
              AND (@FromDate IS NULL OR a.CreatedAt >= @FromDate)
              AND (@ToDate IS NULL OR a.CreatedAt < DATEADD(DAY, 1, @ToDate))
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY a.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM AuditLogs a
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.UserId,
            __request.Action,
            __request.EntityName,
            __request.FromDate,
            __request.ToDate,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<AuditLogModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<AuditLogModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }
}
