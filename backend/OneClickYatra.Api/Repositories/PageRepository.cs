using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class PageRepository : IPageRepository
{
    private const string SelectColumns = """
        SELECT Id, Slug, Title, Content, IsPublished, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
        FROM Pages
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PageRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<PageModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Id = @Id AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PageModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PageModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Slug = @Slug AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PageModel>(
            new CommandDefinition(sql, new { Slug = __slug }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<PageModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE IsDeleted = 0
              AND (@SearchTerm IS NULL OR Title LIKE '%' + @SearchTerm + '%' OR Slug LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY Title ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Pages
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { __request.SearchTerm, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<PageModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<PageModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(PageModel __page, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Pages (Id, Slug, Title, Content, IsPublished, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @Slug, @Title, @Content, @IsPublished, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __page, cancellationToken: __cancellationToken));
        return __page.Id;
    }

    public async Task UpdateAsync(PageModel __page, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Pages
            SET Slug = @Slug, Title = @Title, Content = @Content, IsPublished = @IsPublished,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __page, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Pages SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
