using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class LeadRepository : ILeadRepository
{
    private const string SelectColumns = """
        SELECT l.Id, l.CustomerName, l.Mobile, l.Email, l.DestinationId, l.TravelDate, l.Budget, l.Source,
               l.AssignedToUserId, l.LeadScore, l.Status, l.CustomerId,
               l.CreatedAt, l.CreatedBy, l.UpdatedAt, l.UpdatedBy, l.IsDeleted,
               d.Name AS DestinationName, u.FullName AS AssignedToName
        FROM Leads l
        LEFT JOIN Destinations d ON d.Id = l.DestinationId
        LEFT JOIN Users u ON u.Id = l.AssignedToUserId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public LeadRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<LeadModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE l.Id = @Id AND l.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<LeadModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<LeadModel>> SearchAsync(LeadSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE l.IsDeleted = 0
              AND (@Status IS NULL OR l.Status = @Status)
              AND (@AssignedToUserId IS NULL OR l.AssignedToUserId = @AssignedToUserId)
              AND (@DestinationId IS NULL OR l.DestinationId = @DestinationId)
              AND (@SearchTerm IS NULL OR l.CustomerName LIKE '%' + @SearchTerm + '%' OR l.Mobile LIKE '%' + @SearchTerm + '%' OR l.Email LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY l.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Leads l
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Status,
            __request.AssignedToUserId,
            __request.DestinationId,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<LeadModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<LeadModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(LeadModel __lead, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Leads
                (Id, CustomerName, Mobile, Email, DestinationId, TravelDate, Budget, Source,
                 AssignedToUserId, LeadScore, Status, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @CustomerName, @Mobile, @Email, @DestinationId, @TravelDate, @Budget, @Source,
                 @AssignedToUserId, @LeadScore, @Status, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __lead, cancellationToken: __cancellationToken));
        return __lead.Id;
    }

    public async Task UpdateAsync(LeadModel __lead, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Leads
            SET CustomerName = @CustomerName, Mobile = @Mobile, Email = @Email, DestinationId = @DestinationId,
                TravelDate = @TravelDate, Budget = @Budget, Source = @Source, AssignedToUserId = @AssignedToUserId,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __lead, cancellationToken: __cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Leads SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task AssignAsync(Guid __id, Guid __assignedToUserId, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Leads SET AssignedToUserId = @AssignedToUserId, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, AssignedToUserId = __assignedToUserId, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task UpdateScoreAsync(Guid __id, int __leadScore, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Leads SET LeadScore = @LeadScore, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, LeadScore = __leadScore, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task LinkCustomerAsync(Guid __id, Guid __customerId, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Leads SET CustomerId = @CustomerId, Status = 'Confirmed', UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, CustomerId = __customerId, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Leads SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
