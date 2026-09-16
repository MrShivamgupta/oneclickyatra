using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class RefundRepository : IRefundRepository
{
    private const string SelectColumns = """
        SELECT Id, PaymentId, BookingId, Amount, Reason, Status, GatewayRefundId, RequestedBy,
               CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
        FROM Refunds
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public RefundRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<RefundModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Id = @Id AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<RefundModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<RefundModel>> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE BookingId = @BookingId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<RefundModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task CreateAsync(RefundModel __refund, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Refunds (Id, PaymentId, BookingId, Amount, Reason, Status, GatewayRefundId, RequestedBy, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @PaymentId, @BookingId, @Amount, @Reason, @Status, @GatewayRefundId, @RequestedBy, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __refund, cancellationToken: __cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, string? __gatewayRefundId, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Refunds
            SET Status = @Status,
                GatewayRefundId = COALESCE(@GatewayRefundId, GatewayRefundId),
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, GatewayRefundId = __gatewayRefundId, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }
}
