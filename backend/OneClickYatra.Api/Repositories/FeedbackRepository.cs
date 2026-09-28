using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class FeedbackRepository : IFeedbackRepository
{
    private const string SelectColumns = """
        SELECT f.Id, f.BookingId, f.CustomerId, f.Rating, f.Comment,
               f.CreatedAt, f.CreatedBy, f.UpdatedAt, f.UpdatedBy, f.IsDeleted,
               b.BookingNumber, c.FullName AS CustomerName
        FROM Feedbacks f
        INNER JOIN Bookings b ON b.Id = f.BookingId
        INNER JOIN Customers c ON c.Id = f.CustomerId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public FeedbackRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<FeedbackModel?> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE f.BookingId = @BookingId AND f.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<FeedbackModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
    }

    public async Task CreateAsync(FeedbackModel __feedback, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Feedbacks (Id, BookingId, CustomerId, Rating, Comment, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @BookingId, @CustomerId, @Rating, @Comment, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __feedback, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<FeedbackModel>> SearchAsync(FeedbackSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE f.IsDeleted = 0
              AND (@MinRating IS NULL OR f.Rating >= @MinRating)
              AND (@MaxRating IS NULL OR f.Rating <= @MaxRating)
              AND (@FromDate IS NULL OR f.CreatedAt >= @FromDate)
              AND (@ToDate IS NULL OR f.CreatedAt < DATEADD(DAY, 1, @ToDate))
              AND (@SearchTerm IS NULL OR b.BookingNumber LIKE '%' + @SearchTerm + '%' OR c.FullName LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY f.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Feedbacks f
            INNER JOIN Bookings b ON b.Id = f.BookingId
            INNER JOIN Customers c ON c.Id = f.CustomerId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.MinRating,
            __request.MaxRating,
            __request.FromDate,
            __request.ToDate,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<FeedbackModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<FeedbackModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<IReadOnlyList<FeedbackModel>> GetPublicTestimonialsAsync(int __minRating, int __limit, CancellationToken __cancellationToken)
    {
        var sql = $"""
            {SelectColumns}
            WHERE f.IsDeleted = 0 AND f.Rating >= @MinRating AND f.Comment IS NOT NULL AND LEN(f.Comment) > 0
            ORDER BY f.CreatedAt DESC
            OFFSET 0 ROWS FETCH NEXT @Limit ROWS ONLY
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<FeedbackModel>(
            new CommandDefinition(sql, new { MinRating = __minRating, Limit = __limit }, cancellationToken: __cancellationToken));
        return result.ToList();
    }
}
