using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PasswordResetTokenRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task CreateAsync(PasswordResetTokenModel __token, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO PasswordResetTokens (Id, UserId, TokenHash, ExpiresAtUtc, CreatedAt)
            VALUES (@Id, @UserId, @TokenHash, @ExpiresAtUtc, SYSUTCDATETIME())
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, __token, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<PasswordResetTokenModel?> GetByTokenHashAsync(string __tokenHash, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, UserId, TokenHash, ExpiresAtUtc, UsedAtUtc, CreatedAt
            FROM PasswordResetTokens
            WHERE TokenHash = @TokenHash
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { TokenHash = __tokenHash }, cancellationToken: __cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PasswordResetTokenModel>(command);
    }

    public async Task MarkUsedAsync(Guid __id, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE PasswordResetTokens SET UsedAtUtc = SYSUTCDATETIME() WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }
}
