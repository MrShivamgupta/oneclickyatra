using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private const string SelectColumns = """
        SELECT b.Id, b.BookingNumber, b.LeadId, b.CustomerId, b.QuotationId, b.QuotationOptionId, b.PackageId,
               b.DestinationId, b.TravelDate, b.ReturnDate, b.NumberOfAdults, b.NumberOfChildren, b.TotalAmount,
               b.AmountPaid, b.Notes, b.Status, b.CancellationReason,
               b.CreatedAt, b.CreatedBy, b.UpdatedAt, b.UpdatedBy, b.IsDeleted,
               c.FullName AS CustomerName, p.Title AS PackageTitle, d.Name AS DestinationName
        FROM Bookings b
        INNER JOIN Customers c ON c.Id = b.CustomerId
        LEFT JOIN Packages p ON p.Id = b.PackageId
        LEFT JOIN Destinations d ON d.Id = b.DestinationId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public BookingRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<BookingModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE b.Id = @Id AND b.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<BookingModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<BookingModel>> SearchAsync(BookingSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE b.IsDeleted = 0
              AND (@Status IS NULL OR b.Status = @Status)
              AND (@CustomerId IS NULL OR b.CustomerId = @CustomerId)
              AND (@SearchTerm IS NULL OR b.BookingNumber LIKE '%' + @SearchTerm + '%' OR c.FullName LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY b.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Bookings b
            INNER JOIN Customers c ON c.Id = b.CustomerId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Status,
            __request.CustomerId,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<BookingModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<BookingModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<IReadOnlyList<BookingModel>> GetUpcomingDeparturesAsync(int __withinDays, int __top, CancellationToken __cancellationToken)
    {
        var sql = $"""
            {SelectColumns}
            WHERE b.IsDeleted = 0 AND b.Status IN ('Confirmed', 'InProgress')
              AND b.TravelDate BETWEEN CAST(SYSUTCDATETIME() AS DATE) AND DATEADD(DAY, @WithinDays, CAST(SYSUTCDATETIME() AS DATE))
            ORDER BY b.TravelDate
            OFFSET 0 ROWS FETCH NEXT @Top ROWS ONLY
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BookingModel>(
            new CommandDefinition(sql, new { WithinDays = __withinDays, Top = __top }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task CreateAsync(BookingModel __booking, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Bookings
                (Id, BookingNumber, LeadId, CustomerId, QuotationId, QuotationOptionId, PackageId, DestinationId,
                 TravelDate, ReturnDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Notes, Status,
                 CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @BookingNumber, @LeadId, @CustomerId, @QuotationId, @QuotationOptionId, @PackageId, @DestinationId,
                 @TravelDate, @ReturnDate, @NumberOfAdults, @NumberOfChildren, @TotalAmount, @AmountPaid, @Notes, @Status,
                 SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __booking, cancellationToken: __cancellationToken));
    }

    public async Task UpdateAsync(BookingModel __booking, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Bookings
            SET PackageId = @PackageId, DestinationId = @DestinationId, TravelDate = @TravelDate, ReturnDate = @ReturnDate,
                NumberOfAdults = @NumberOfAdults, NumberOfChildren = @NumberOfChildren, TotalAmount = @TotalAmount,
                Notes = @Notes, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __booking, cancellationToken: __cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Bookings SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task AddAmountPaidAsync(Guid __id, decimal __amount, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Bookings SET AmountPaid = AmountPaid + @Amount, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Amount = __amount }, cancellationToken: __cancellationToken));
    }

    public async Task SetCancellationReasonAsync(Guid __id, string? __reason, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Bookings SET CancellationReason = @Reason WHERE Id = @Id AND IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Reason = __reason }, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Bookings SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<BookingPassengerModel>> GetPassengersAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, BookingId, FullName, Age, Gender, IdProofType, IdProofNumber, IsLeadPassenger
            FROM BookingPassengers WHERE BookingId = @BookingId AND IsDeleted = 0
            ORDER BY IsLeadPassenger DESC, FullName
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BookingPassengerModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplacePassengersAsync(Guid __bookingId, IReadOnlyList<BookingPassengerModel> __passengers, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM BookingPassengers WHERE BookingId = @BookingId",
            new { BookingId = __bookingId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO BookingPassengers (Id, BookingId, FullName, Age, Gender, IdProofType, IdProofNumber, IsLeadPassenger, CreatedAt, IsDeleted)
            VALUES (@Id, @BookingId, @FullName, @Age, @Gender, @IdProofType, @IdProofNumber, @IsLeadPassenger, SYSUTCDATETIME(), 0)
            """;
        foreach (var passenger in __passengers)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                BookingId = __bookingId,
                passenger.FullName,
                passenger.Age,
                passenger.Gender,
                passenger.IdProofType,
                passenger.IdProofNumber,
                passenger.IsLeadPassenger
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<BookingAddOnModel>> GetAddOnsAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, BookingId, Name, Description, Price, Quantity
            FROM BookingAddOns WHERE BookingId = @BookingId AND IsDeleted = 0
            ORDER BY Name
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BookingAddOnModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceAddOnsAsync(Guid __bookingId, IReadOnlyList<BookingAddOnModel> __addOns, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM BookingAddOns WHERE BookingId = @BookingId",
            new { BookingId = __bookingId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO BookingAddOns (Id, BookingId, Name, Description, Price, Quantity, CreatedAt, IsDeleted)
            VALUES (@Id, @BookingId, @Name, @Description, @Price, @Quantity, SYSUTCDATETIME(), 0)
            """;
        foreach (var addOn in __addOns)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                BookingId = __bookingId,
                addOn.Name,
                addOn.Description,
                addOn.Price,
                addOn.Quantity
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task AddStatusHistoryAsync(BookingStatusHistoryModel __history, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO BookingStatusHistory (Id, BookingId, OldStatus, NewStatus, ChangedBy, ChangedAt, Reason, TrackingId)
            VALUES (@Id, @BookingId, @OldStatus, @NewStatus, @ChangedBy, SYSUTCDATETIME(), @Reason, @TrackingId)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __history, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<BookingStatusHistoryModel>> GetStatusHistoryAsync(Guid __bookingId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT h.Id, h.BookingId, h.OldStatus, h.NewStatus, h.ChangedBy, h.ChangedAt, h.Reason, h.TrackingId,
                   u.FullName AS ChangedByName
            FROM BookingStatusHistory h
            LEFT JOIN Users u ON u.Id = h.ChangedBy
            WHERE h.BookingId = @BookingId
            ORDER BY h.ChangedAt DESC
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<BookingStatusHistoryModel>(
            new CommandDefinition(sql, new { BookingId = __bookingId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }
}
