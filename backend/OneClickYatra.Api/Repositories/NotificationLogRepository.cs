using Dapper;
using Microsoft.Data.SqlClient;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class NotificationLogRepository : INotificationLogRepository
{
    private const int UniqueConstraintViolation1 = 2601;
    private const int UniqueConstraintViolation2 = 2627;

    private const string SelectColumns = """
        SELECT n.Id, n.Channel, n.RecipientPhone, n.RecipientEmail, n.TemplateId, n.Subject, n.Body, n.Status,
               n.GatewayMessageId, n.ErrorMessage, n.SentAt, n.CreatedAt, n.CreatedBy, n.UpdatedAt, n.UpdatedBy, n.IsDeleted,
               t.Name AS TemplateName
        FROM NotificationLogs n
        LEFT JOIN WhatsAppTemplates t ON t.Id = n.TemplateId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public NotificationLogRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<NotificationLogModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE n.Id = @Id AND n.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<NotificationLogModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<NotificationLogModel>> SearchAsync(NotificationLogSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE n.IsDeleted = 0
              AND (@Channel IS NULL OR n.Channel = @Channel)
              AND (@Status IS NULL OR n.Status = @Status)
              AND (@SearchTerm IS NULL OR n.RecipientPhone LIKE '%' + @SearchTerm + '%'
                   OR n.RecipientEmail LIKE '%' + @SearchTerm + '%' OR t.Name LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY n.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM NotificationLogs n
            LEFT JOIN WhatsAppTemplates t ON t.Id = n.TemplateId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Channel,
            __request.Status,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<NotificationLogModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<NotificationLogModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<bool> CreateAsync(NotificationLogModel __log, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO NotificationLogs
                (Id, Channel, RecipientPhone, RecipientEmail, TemplateId, Subject, Body, Status, GatewayMessageId, ErrorMessage, SentAt, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @Channel, @RecipientPhone, @RecipientEmail, @TemplateId, @Subject, @Body, @Status, @GatewayMessageId, @ErrorMessage, @SentAt, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(sql, __log, cancellationToken: __cancellationToken));
            return true;
        }
        catch (SqlException ex) when (ex.Number is UniqueConstraintViolation1 or UniqueConstraintViolation2)
        {
            return false;
        }
    }
}
