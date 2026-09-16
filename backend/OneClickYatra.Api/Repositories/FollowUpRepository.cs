using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class FollowUpRepository : IFollowUpRepository
{
    private const string SelectColumns = """
        SELECT f.Id, f.LeadId, f.ScheduledAt, f.Type, f.Notes, f.Status, f.CompletedAt,
               f.CreatedAt, f.CreatedBy, f.UpdatedAt, f.UpdatedBy, f.IsDeleted,
               l.CustomerName AS LeadCustomerName
        FROM FollowUps f
        INNER JOIN Leads l ON l.Id = f.LeadId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public FollowUpRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<FollowUpModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE f.Id = @Id AND f.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<FollowUpModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<FollowUpModel>> ListByLeadAsync(Guid __leadId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE f.LeadId = @LeadId AND f.IsDeleted = 0 ORDER BY f.ScheduledAt";
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<FollowUpModel>(
            new CommandDefinition(sql, new { LeadId = __leadId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<PaginationResponse<FollowUpModel>> ListTodayAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE f.IsDeleted = 0
              AND f.Status = 'Pending'
              AND CAST(f.ScheduledAt AS DATE) = CAST(SYSUTCDATETIME() AS DATE)
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY f.ScheduledAt
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM FollowUps f
            INNER JOIN Leads l ON l.Id = f.LeadId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<FollowUpModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<FollowUpModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(FollowUpModel __followUp, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO FollowUps (Id, LeadId, ScheduledAt, Type, Notes, Status, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @LeadId, @ScheduledAt, @Type, @Notes, @Status, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __followUp, cancellationToken: __cancellationToken));
        return __followUp.Id;
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, string? __notes, DateTime? __completedAt, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE FollowUps
            SET Status = @Status,
                Notes = COALESCE(@Notes, Notes),
                CompletedAt = @CompletedAt,
                UpdatedAt = SYSUTCDATETIME(),
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = __id,
            Status = __status,
            Notes = __notes,
            CompletedAt = __completedAt,
            UpdatedBy = __updatedBy
        }, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE FollowUps SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
