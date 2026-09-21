using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class CustomerDocumentRepository : ICustomerDocumentRepository
{
    private const string SelectColumns = """
        SELECT Id, BookingId, UploadedByUserId, FileName, ContentType, FileSizeBytes, StoragePath, CreatedAt, CreatedBy
        FROM CustomerDocuments
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerDocumentRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task CreateAsync(CustomerDocumentModel __document, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO CustomerDocuments (Id, BookingId, UploadedByUserId, FileName, ContentType, FileSizeBytes, StoragePath, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @BookingId, @UploadedByUserId, @FileName, @ContentType, @FileSizeBytes, @StoragePath, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __document, cancellationToken: __cancellationToken));
    }

    public async Task<CustomerDocumentModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE Id = @Id AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CustomerDocumentModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<CustomerDocumentModel>> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE BookingId = @BookingId AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<CustomerDocumentModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }
}
