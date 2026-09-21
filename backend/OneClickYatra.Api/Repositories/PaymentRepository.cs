using Dapper;
using Microsoft.Data.SqlClient;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private const int UniqueConstraintViolation1 = 2601;
    private const int UniqueConstraintViolation2 = 2627;

    private const string SelectColumns = """
        SELECT p.Id, p.BookingId, p.Amount, p.Currency, p.Status, p.GatewayProvider, p.GatewayOrderId,
               p.GatewayPaymentId, p.Notes, p.CreatedAt, p.CreatedBy, p.UpdatedAt, p.UpdatedBy, p.IsDeleted,
               b.BookingNumber, c.FullName AS CustomerName
        FROM Payments p
        INNER JOIN Bookings b ON b.Id = p.BookingId
        INNER JOIN Customers c ON c.Id = b.CustomerId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public PaymentRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<PaymentModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE p.Id = @Id AND p.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PaymentModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PaymentModel?> GetByGatewayOrderIdAsync(string __gatewayOrderId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE p.GatewayOrderId = @GatewayOrderId AND p.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<PaymentModel>(
            new CommandDefinition(sql, new { GatewayOrderId = __gatewayOrderId }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<PaymentModel>> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE p.BookingId = @BookingId AND p.IsDeleted = 0 ORDER BY p.CreatedAt DESC";
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PaymentModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<PaginationResponse<PaymentModel>> SearchAsync(PaymentSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE p.IsDeleted = 0
              AND (@Status IS NULL OR p.Status = @Status)
              AND (@BookingId IS NULL OR p.BookingId = @BookingId)
              AND (@CustomerId IS NULL OR b.CustomerId = @CustomerId)
              AND (@SearchTerm IS NULL OR b.BookingNumber LIKE '%' + @SearchTerm + '%' OR c.FullName LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY p.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Payments p
            INNER JOIN Bookings b ON b.Id = p.BookingId
            INNER JOIN Customers c ON c.Id = b.CustomerId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Status,
            __request.BookingId,
            __request.CustomerId,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<PaymentModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<PaymentModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task CreateAsync(PaymentModel __payment, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Payments
                (Id, BookingId, Amount, Currency, Status, GatewayProvider, GatewayOrderId, GatewayPaymentId, Notes, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @BookingId, @Amount, @Currency, @Status, @GatewayProvider, @GatewayOrderId, @GatewayPaymentId, @Notes, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __payment, cancellationToken: __cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, string? __gatewayPaymentId, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Payments
            SET Status = @Status,
                GatewayPaymentId = COALESCE(@GatewayPaymentId, GatewayPaymentId),
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, GatewayPaymentId = __gatewayPaymentId }, cancellationToken: __cancellationToken));
    }

    public async Task<bool> AddTransactionAsync(PaymentTransactionModel __transaction, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO PaymentTransactions (Id, PaymentId, EventType, GatewayEventId, RawPayload, CreatedAt)
            VALUES (@Id, @PaymentId, @EventType, @GatewayEventId, @RawPayload, SYSUTCDATETIME())
            """;
        using var connection = _connectionFactory.CreateConnection();

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(sql, __transaction, cancellationToken: __cancellationToken));
            return true;
        }
        catch (SqlException ex) when (ex.Number is UniqueConstraintViolation1 or UniqueConstraintViolation2)
        {
            return false;
        }
    }
}
