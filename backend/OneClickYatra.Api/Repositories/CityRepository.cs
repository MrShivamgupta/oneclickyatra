using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class CityRepository : ICityRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CityRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<CityModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT c.Id, c.CountryId, c.Name, c.CreatedAt, c.CreatedBy, c.UpdatedAt, c.UpdatedBy, c.IsDeleted,
                   co.Name AS CountryName
            FROM Cities c
            INNER JOIN Countries co ON co.Id = c.CountryId
            WHERE c.Id = @Id AND c.IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CityModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<CityModel?> GetByCountryAndNameAsync(Guid __countryId, string __name, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, CountryId, Name, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Cities WHERE CountryId = @CountryId AND Name = @Name AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CityModel>(
            new CommandDefinition(sql, new { CountryId = __countryId, Name = __name }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<CityModel>> ListAsync(PaginationRequest __request, Guid? __countryId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT c.Id, c.CountryId, c.Name, c.CreatedAt, c.CreatedBy, c.UpdatedAt, c.UpdatedBy, c.IsDeleted,
                   co.Name AS CountryName
            FROM Cities c
            INNER JOIN Countries co ON co.Id = c.CountryId
            WHERE c.IsDeleted = 0
              AND (@CountryId IS NULL OR c.CountryId = @CountryId)
              AND (@SearchTerm IS NULL OR c.Name LIKE '%' + @SearchTerm + '%')
            ORDER BY c.Name ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM Cities c
            WHERE c.IsDeleted = 0
              AND (@CountryId IS NULL OR c.CountryId = @CountryId)
              AND (@SearchTerm IS NULL OR c.Name LIKE '%' + @SearchTerm + '%');
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            CountryId = __countryId,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<CityModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<CityModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(CityModel __city, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Cities (Id, CountryId, Name, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @CountryId, @Name, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __city, cancellationToken: __cancellationToken));
        return __city.Id;
    }

    public async Task UpdateAsync(CityModel __city, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Cities
            SET CountryId = @CountryId, Name = @Name, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __city, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Cities SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
