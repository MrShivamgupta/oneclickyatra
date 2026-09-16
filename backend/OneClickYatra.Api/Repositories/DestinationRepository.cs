using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class DestinationRepository : IDestinationRepository
{
    private const string SelectColumns = """
        SELECT d.Id, d.CountryId, d.CityId, d.Name, d.Slug, d.ShortDescription, d.Description,
               d.HeroImageUrl, d.IsFeatured, d.IsPublished,
               d.CreatedAt, d.CreatedBy, d.UpdatedAt, d.UpdatedBy, d.IsDeleted,
               co.Name AS CountryName, ci.Name AS CityName
        FROM Destinations d
        INNER JOIN Countries co ON co.Id = d.CountryId
        LEFT JOIN Cities ci ON ci.Id = d.CityId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public DestinationRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<DestinationModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE d.Id = @Id AND d.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DestinationModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<DestinationModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE d.Slug = @Slug AND d.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<DestinationModel>(
            new CommandDefinition(sql, new { Slug = __slug }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<DestinationModel>> SearchAsync(DestinationSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE d.IsDeleted = 0
              AND (@CountryId IS NULL OR d.CountryId = @CountryId)
              AND (@IsFeatured IS NULL OR d.IsFeatured = @IsFeatured)
              AND (@IsPublished IS NULL OR d.IsPublished = @IsPublished)
              AND (@SearchTerm IS NULL OR d.Name LIKE '%' + @SearchTerm + '%' OR co.Name LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY d.IsFeatured DESC, d.Name ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Destinations d
            INNER JOIN Countries co ON co.Id = d.CountryId
            LEFT JOIN Cities ci ON ci.Id = d.CityId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.CountryId,
            __request.IsFeatured,
            __request.IsPublished,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<DestinationModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<DestinationModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(DestinationModel __destination, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Destinations
                (Id, CountryId, CityId, Name, Slug, ShortDescription, Description, HeroImageUrl, IsFeatured, IsPublished, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @CountryId, @CityId, @Name, @Slug, @ShortDescription, @Description, @HeroImageUrl, @IsFeatured, @IsPublished, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __destination, cancellationToken: __cancellationToken));
        return __destination.Id;
    }

    public async Task UpdateAsync(DestinationModel __destination, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Destinations
            SET CountryId = @CountryId, CityId = @CityId, Name = @Name, Slug = @Slug,
                ShortDescription = @ShortDescription, Description = @Description, HeroImageUrl = @HeroImageUrl,
                IsFeatured = @IsFeatured, IsPublished = @IsPublished,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __destination, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Destinations SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
