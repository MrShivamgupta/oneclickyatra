using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class PackageRepository : IPackageRepository
{
    private const string SelectColumns = """
        SELECT p.Id, p.DestinationId, p.CategoryId, p.SeasonId, p.Title, p.Slug, p.DurationDays, p.DurationNights,
               p.ShortDescription, p.Description, p.HeroImageUrl, p.Status,
               p.CreatedAt, p.CreatedBy, p.UpdatedAt, p.UpdatedBy, p.IsDeleted,
               d.Name AS DestinationName, c.Name AS CategoryName, s.Name AS SeasonName,
               (SELECT MIN(pr.PricePerPerson) FROM PackagePricing pr WHERE pr.PackageId = p.Id) AS StartingPricePerPerson,
               (SELECT TOP 1 pr.Currency FROM PackagePricing pr WHERE pr.PackageId = p.Id ORDER BY pr.PricePerPerson ASC) AS PriceCurrency
        FROM Packages p
        INNER JOIN Destinations d ON d.Id = p.DestinationId
        LEFT JOIN Categories c ON c.Id = p.CategoryId
        LEFT JOIN Seasons s ON s.Id = p.SeasonId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PackageRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<PackageModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE p.Id = @Id AND p.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PackageModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    /// <summary>Batched existence check — see IDestinationRepository.GetExistingIdsAsync for the
    /// same rationale (one round trip instead of one GetByIdAsync per item in a validation loop).</summary>
    public async Task<IReadOnlyList<Guid>> GetExistingIdsAsync(IReadOnlyList<Guid> __ids, CancellationToken __cancellationToken)
    {
        if (__ids.Count == 0)
        {
            return [];
        }

        const string sql = "SELECT Id FROM Packages WHERE Id IN @Ids AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<Guid>(new CommandDefinition(sql, new { Ids = __ids }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<PackageModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE p.Slug = @Slug AND p.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PackageModel>(
            new CommandDefinition(sql, new { Slug = __slug }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<PackageModel>> SearchAsync(PackageSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE p.IsDeleted = 0
              AND (@DestinationId IS NULL OR p.DestinationId = @DestinationId)
              AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
              AND (@Status IS NULL OR p.Status = @Status)
              AND (@SearchTerm IS NULL OR p.Title LIKE '%' + @SearchTerm + '%' OR d.Name LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY p.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Packages p
            INNER JOIN Destinations d ON d.Id = p.DestinationId
            LEFT JOIN Categories c ON c.Id = p.CategoryId
            LEFT JOIN Seasons s ON s.Id = p.SeasonId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.DestinationId,
            __request.CategoryId,
            __request.Status,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<PackageModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<PackageModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(PackageModel __package, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Packages
                (Id, DestinationId, CategoryId, SeasonId, Title, Slug, DurationDays, DurationNights,
                 ShortDescription, Description, HeroImageUrl, Status, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @DestinationId, @CategoryId, @SeasonId, @Title, @Slug, @DurationDays, @DurationNights,
                 @ShortDescription, @Description, @HeroImageUrl, @Status, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __package, cancellationToken: __cancellationToken));
        return __package.Id;
    }

    public async Task UpdateAsync(PackageModel __package, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Packages
            SET DestinationId = @DestinationId, CategoryId = @CategoryId, SeasonId = @SeasonId,
                Title = @Title, Slug = @Slug, DurationDays = @DurationDays, DurationNights = @DurationNights,
                ShortDescription = @ShortDescription, Description = @Description, HeroImageUrl = @HeroImageUrl,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __package, cancellationToken: __cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Packages
            SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Packages SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
