using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task CreateAsync(RefreshTokenModel __token, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO RefreshTokens (Id, UserId, TokenHash, ExpiresAtUtc, CreatedByIp, CreatedAt)
            VALUES (@Id, @UserId, @TokenHash, @ExpiresAtUtc, @CreatedByIp, SYSUTCDATETIME())
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, __token, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<RefreshTokenModel?> GetByTokenHashAsync(string __tokenHash, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, UserId, TokenHash, ExpiresAtUtc, RevokedAtUtc, ReplacedByTokenHash, CreatedByIp, CreatedAt
            FROM RefreshTokens
            WHERE TokenHash = @TokenHash
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { TokenHash = __tokenHash }, cancellationToken: __cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<RefreshTokenModel>(command);
    }

    public async Task RevokeAsync(Guid __id, string? __replacedByTokenHash, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE RefreshTokens
            SET RevokedAtUtc = SYSUTCDATETIME(), ReplacedByTokenHash = @ReplacedByTokenHash
            WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = __id, ReplacedByTokenHash = __replacedByTokenHash }, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task RevokeAllForUserAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE RefreshTokens
            SET RevokedAtUtc = SYSUTCDATETIME()
            WHERE UserId = @UserId AND RevokedAtUtc IS NULL
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId }, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<int> DeleteExpiredAsync(CancellationToken __cancellationToken)
    {
        const string sql = """
            DELETE FROM RefreshTokens
            WHERE ExpiresAtUtc < SYSUTCDATETIME() OR RevokedAtUtc IS NOT NULL
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: __cancellationToken);
        return await connection.ExecuteAsync(command);
    }
}
