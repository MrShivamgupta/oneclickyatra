using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class EnquiryRepository : IEnquiryRepository
{
    private const string SelectColumns = """
        SELECT e.Id, e.FullName, e.Email, e.Phone, e.DestinationId, e.TravelDate, e.Message, e.Status,
               e.CreatedAt, e.CreatedBy, e.UpdatedAt, e.UpdatedBy, e.IsDeleted,
               d.Name AS DestinationName
        FROM Enquiries e
        LEFT JOIN Destinations d ON d.Id = e.DestinationId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public EnquiryRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<EnquiryModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE e.Id = @Id AND e.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<EnquiryModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<EnquiryModel>> SearchAsync(EnquirySearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE e.IsDeleted = 0
              AND (@Status IS NULL OR e.Status = @Status)
              AND (@SearchTerm IS NULL OR e.FullName LIKE '%' + @SearchTerm + '%' OR e.Email LIKE '%' + @SearchTerm + '%' OR e.Phone LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY e.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Enquiries e
            LEFT JOIN Destinations d ON d.Id = e.DestinationId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Status,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<EnquiryModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<EnquiryModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(EnquiryModel __enquiry, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Enquiries
                (Id, FullName, Email, Phone, DestinationId, TravelDate, Message, Status, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @FullName, @Email, @Phone, @DestinationId, @TravelDate, @Message, @Status, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __enquiry, cancellationToken: __cancellationToken));
        return __enquiry.Id;
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Enquiries
            SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Enquiries SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
