using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private const string SelectColumns = """
        SELECT i.Id, i.BookingId, i.InvoiceNumber, i.Amount, i.IssuedAt,
               i.CreatedAt, i.CreatedBy, i.UpdatedAt, i.UpdatedBy, i.IsDeleted,
               b.BookingNumber, c.FullName AS CustomerName
        FROM Invoices i
        INNER JOIN Bookings b ON b.Id = i.BookingId
        INNER JOIN Customers c ON c.Id = b.CustomerId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<InvoiceModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE i.Id = @Id AND i.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<InvoiceModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<InvoiceModel?> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE i.BookingId = @BookingId AND i.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<InvoiceModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<InvoiceModel>> SearchAsync(InvoiceSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE i.IsDeleted = 0
              AND (@BookingId IS NULL OR i.BookingId = @BookingId)
              AND (@SearchTerm IS NULL OR i.InvoiceNumber LIKE '%' + @SearchTerm + '%' OR b.BookingNumber LIKE '%' + @SearchTerm + '%' OR c.FullName LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY i.IssuedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Invoices i
            INNER JOIN Bookings b ON b.Id = i.BookingId
            INNER JOIN Customers c ON c.Id = b.CustomerId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.BookingId,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<InvoiceModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<InvoiceModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task CreateAsync(InvoiceModel __invoice, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Invoices (Id, BookingId, InvoiceNumber, Amount, IssuedAt, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @BookingId, @InvoiceNumber, @Amount, @IssuedAt, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __invoice, cancellationToken: __cancellationToken));
    }
}
