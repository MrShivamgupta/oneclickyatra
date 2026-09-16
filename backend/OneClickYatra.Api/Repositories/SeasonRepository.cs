using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class SeasonRepository : ISeasonRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SeasonRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<SeasonModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, StartMonth, EndMonth, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Seasons WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SeasonModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<SeasonModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, StartMonth, EndMonth, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Seasons WHERE Name = @Name AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<SeasonModel>(
            new CommandDefinition(sql, new { Name = __name }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<SeasonModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, StartMonth, EndMonth, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Seasons
            WHERE IsDeleted = 0 AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%')
            ORDER BY StartMonth ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM Seasons
            WHERE IsDeleted = 0 AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%');
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { __request.SearchTerm, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<SeasonModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<SeasonModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(SeasonModel __season, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Seasons (Id, Name, StartMonth, EndMonth, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @Name, @StartMonth, @EndMonth, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __season, cancellationToken: __cancellationToken));
        return __season.Id;
    }

    public async Task UpdateAsync(SeasonModel __season, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Seasons
            SET Name = @Name, StartMonth = @StartMonth, EndMonth = @EndMonth, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __season, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Seasons SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
