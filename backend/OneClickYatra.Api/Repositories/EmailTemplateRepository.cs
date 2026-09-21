using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class EmailTemplateRepository : IEmailTemplateRepository
{
    private const string SelectColumns = """
        SELECT Id, Name, Subject, BodyHtml, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
        FROM EmailTemplates
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public EmailTemplateRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<EmailTemplateModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Id = @Id AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EmailTemplateModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<EmailTemplateModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Name = @Name AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EmailTemplateModel>(
            new CommandDefinition(sql, new { Name = __name }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<EmailTemplateModel>> SearchAsync(EmailTemplateSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE IsDeleted = 0
              AND (@IsActive IS NULL OR IsActive = @IsActive)
              AND (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%' OR Subject LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY Name ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM EmailTemplates
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.IsActive,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<EmailTemplateModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<EmailTemplateModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task CreateAsync(EmailTemplateModel __template, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO EmailTemplates (Id, Name, Subject, BodyHtml, IsActive, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @Name, @Subject, @BodyHtml, @IsActive, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __template, cancellationToken: __cancellationToken));
    }

    public async Task UpdateAsync(EmailTemplateModel __template, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE EmailTemplates
            SET Name = @Name, Subject = @Subject, BodyHtml = @BodyHtml, IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __template, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE EmailTemplates SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
