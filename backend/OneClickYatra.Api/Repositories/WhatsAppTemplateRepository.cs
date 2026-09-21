using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class WhatsAppTemplateRepository : IWhatsAppTemplateRepository
{
    private const string SelectColumns = """
        SELECT Id, Name, Category, BodyText, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
        FROM WhatsAppTemplates
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public WhatsAppTemplateRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<WhatsAppTemplateModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Id = @Id AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<WhatsAppTemplateModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<WhatsAppTemplateModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Name = @Name AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<WhatsAppTemplateModel>(
            new CommandDefinition(sql, new { Name = __name }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<WhatsAppTemplateModel>> SearchAsync(WhatsAppTemplateSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE IsDeleted = 0
              AND (@Category IS NULL OR Category = @Category)
              AND (@IsActive IS NULL OR IsActive = @IsActive)
              AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%' OR BodyText LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY Name ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM WhatsAppTemplates
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Category,
            __request.IsActive,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<WhatsAppTemplateModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<WhatsAppTemplateModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task CreateAsync(WhatsAppTemplateModel __template, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO WhatsAppTemplates (Id, Name, Category, BodyText, IsActive, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @Name, @Category, @BodyText, @IsActive, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __template, cancellationToken: __cancellationToken));
    }

    public async Task UpdateAsync(WhatsAppTemplateModel __template, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE WhatsAppTemplates
            SET Name = @Name, Category = @Category, BodyText = @BodyText, IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __template, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE WhatsAppTemplates SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
