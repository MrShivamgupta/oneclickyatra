using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<CategoryModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, Slug, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Categories WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CategoryModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<CategoryModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, Slug, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Categories WHERE Slug = @Slug AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CategoryModel>(
            new CommandDefinition(sql, new { Slug = __slug }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<CategoryModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, Slug, Description, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Categories
            WHERE IsDeleted = 0 AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%')
            ORDER BY Name ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM Categories
            WHERE IsDeleted = 0 AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%');
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { __request.SearchTerm, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<CategoryModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<CategoryModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(CategoryModel __category, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Categories (Id, Name, Slug, Description, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @Name, @Slug, @Description, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __category, cancellationToken: __cancellationToken));
        return __category.Id;
    }

    public async Task UpdateAsync(CategoryModel __category, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Categories
            SET Name = @Name, Slug = @Slug, Description = @Description, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __category, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Categories SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
