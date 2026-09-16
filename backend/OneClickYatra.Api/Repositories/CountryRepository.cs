using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class CountryRepository : ICountryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CountryRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<CountryModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, IsoCode, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Countries WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CountryModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<CountryModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, IsoCode, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Countries WHERE Name = @Name AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CountryModel>(
            new CommandDefinition(sql, new { Name = __name }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<CountryModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var sortColumn = __request.SortBy switch
        {
            "isoCode" => "IsoCode",
            _ => "Name"
        };
        var direction = __request.SortDescending ? "DESC" : "ASC";

        var sql = $"""
            SELECT Id, Name, IsoCode, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Countries
            WHERE IsDeleted = 0 AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%' OR IsoCode LIKE '%' + @SearchTerm + '%')
            ORDER BY {sortColumn} {direction}
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM Countries
            WHERE IsDeleted = 0 AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%' OR IsoCode LIKE '%' + @SearchTerm + '%');
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<CountryModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<CountryModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(CountryModel __country, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Countries (Id, Name, IsoCode, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @Name, @IsoCode, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __country, cancellationToken: __cancellationToken));
        return __country.Id;
    }

    public async Task UpdateAsync(CountryModel __country, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Countries
            SET Name = @Name, IsoCode = @IsoCode, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __country, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Countries
            SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy
            WHERE Id = @Id
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
