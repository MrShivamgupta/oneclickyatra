using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
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
}
