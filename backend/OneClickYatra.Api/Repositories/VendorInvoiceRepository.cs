using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class VendorInvoiceRepository : IVendorInvoiceRepository
{
    private const string SelectColumns = """
        SELECT vi.Id, vi.VendorId, vi.BookingId, b.BookingNumber, vi.UploadedByUserId, vi.FileName, vi.ContentType,
               vi.FileSizeBytes, vi.StoragePath, vi.Amount, vi.Notes, vi.Status, vi.CreatedAt, vi.CreatedBy, vi.UpdatedAt, vi.UpdatedBy
        FROM VendorInvoices vi
        LEFT JOIN Bookings b ON b.Id = vi.BookingId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public VendorInvoiceRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task CreateAsync(VendorInvoiceModel __invoice, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO VendorInvoices (Id, VendorId, BookingId, UploadedByUserId, FileName, ContentType, FileSizeBytes, StoragePath, Amount, Notes, Status, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @VendorId, @BookingId, @UploadedByUserId, @FileName, @ContentType, @FileSizeBytes, @StoragePath, @Amount, @Notes, @Status, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __invoice, cancellationToken: __cancellationToken));
    }

    public async Task<VendorInvoiceModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE vi.Id = @Id AND vi.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<VendorInvoiceModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<VendorInvoiceModel>> SearchByVendorIdAsync(Guid __vendorId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var sql = $"""
            {SelectColumns}
            WHERE vi.VendorId = @VendorId AND vi.IsDeleted = 0
            ORDER BY vi.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM VendorInvoices vi WHERE vi.VendorId = @VendorId AND vi.IsDeleted = 0;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { VendorId = __vendorId, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<VendorInvoiceModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<VendorInvoiceModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<PaginationResponse<VendorInvoiceModel>> SearchAllAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var sql = $"""
            {SelectColumns}
            WHERE vi.IsDeleted = 0
            ORDER BY vi.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM VendorInvoices vi WHERE vi.IsDeleted = 0;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<VendorInvoiceModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<VendorInvoiceModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE VendorInvoices
            SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }
}
