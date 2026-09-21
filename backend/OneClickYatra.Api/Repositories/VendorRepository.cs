using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class VendorRepository : IVendorRepository
{
    private const string SelectColumns = """
        SELECT v.Id, v.Name, v.VendorType, v.Email, v.Phone, v.Address, v.City, v.Country,
               v.UserId, v.Rating, v.IsActive, v.CreatedAt, v.CreatedBy, v.UpdatedAt, v.UpdatedBy, v.IsDeleted
        FROM Vendors v
        """;

    private const string PaymentSelectColumns = """
        SELECT vp.Id, vp.VendorId, vp.BookingId, b.BookingNumber, vp.Amount, vp.Status, vp.Notes, vp.PaidAt,
               vp.CreatedAt, vp.CreatedBy, vp.UpdatedAt, vp.UpdatedBy, vp.IsDeleted
        FROM VendorPayments vp
        LEFT JOIN Bookings b ON b.Id = vp.BookingId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public VendorRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<VendorModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE v.Id = @Id AND v.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<VendorModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<VendorModel?> GetByUserIdAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE v.UserId = @UserId AND v.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<VendorModel>(
            new CommandDefinition(sql, new { UserId = __userId }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<VendorModel>> SearchAsync(VendorSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE v.IsDeleted = 0
              AND (@VendorType IS NULL OR v.VendorType = @VendorType)
              AND (@IsActive IS NULL OR v.IsActive = @IsActive)
              AND (@SearchTerm IS NULL OR v.Name LIKE '%' + @SearchTerm + '%' OR v.Email LIKE '%' + @SearchTerm + '%' OR v.Phone LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY v.Name ASC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Vendors v
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.VendorType,
            __request.IsActive,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<VendorModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<VendorModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task CreateAsync(VendorModel __vendor, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Vendors
                (Id, Name, VendorType, Email, Phone, Address, City, Country, UserId, Rating, IsActive, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @Name, @VendorType, @Email, @Phone, @Address, @City, @Country, @UserId, @Rating, @IsActive, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __vendor, cancellationToken: __cancellationToken));
    }

    public async Task UpdateAsync(VendorModel __vendor, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Vendors
            SET Name = @Name, VendorType = @VendorType, Email = @Email, Phone = @Phone,
                Address = @Address, City = @City, Country = @Country, IsActive = @IsActive,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __vendor, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Vendors SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }

    public async Task LinkUserAsync(Guid __id, Guid __userId, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Vendors SET UserId = @UserId, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, UserId = __userId, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task RecomputeRatingAsync(Guid __vendorId, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Vendors
            SET Rating = (
                    SELECT AVG(CAST(vp.Rating AS DECIMAL(3, 2)))
                    FROM VendorPerformance vp
                    WHERE vp.VendorId = @VendorId AND vp.IsDeleted = 0
                ),
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @VendorId AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { VendorId = __vendorId }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<VendorContactModel>> GetContactsAsync(Guid __vendorId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, VendorId, ContactName, Designation, Phone, Email, IsPrimary, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM VendorContacts WHERE VendorId = @VendorId AND IsDeleted = 0
            ORDER BY IsPrimary DESC, ContactName
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<VendorContactModel>(
            new CommandDefinition(sql, new { VendorId = __vendorId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceContactsAsync(Guid __vendorId, IReadOnlyList<VendorContactModel> __contacts, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM VendorContacts WHERE VendorId = @VendorId",
            new { VendorId = __vendorId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO VendorContacts (Id, VendorId, ContactName, Designation, Phone, Email, IsPrimary, CreatedAt, IsDeleted)
            VALUES (@Id, @VendorId, @ContactName, @Designation, @Phone, @Email, @IsPrimary, SYSUTCDATETIME(), 0)
            """;
        foreach (var contact in __contacts)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                VendorId = __vendorId,
                contact.ContactName,
                contact.Designation,
                contact.Phone,
                contact.Email,
                contact.IsPrimary
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<VendorRateModel>> GetRatesAsync(Guid __vendorId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT vr.Id, vr.VendorId, vr.DestinationId, d.Name AS DestinationName, vr.ServiceDescription,
                   vr.RateAmount, vr.Currency, vr.ValidFrom, vr.ValidTo,
                   vr.CreatedAt, vr.CreatedBy, vr.UpdatedAt, vr.UpdatedBy, vr.IsDeleted
            FROM VendorRates vr
            LEFT JOIN Destinations d ON d.Id = vr.DestinationId
            WHERE vr.VendorId = @VendorId AND vr.IsDeleted = 0
            ORDER BY vr.ServiceDescription
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<VendorRateModel>(
            new CommandDefinition(sql, new { VendorId = __vendorId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceRatesAsync(Guid __vendorId, IReadOnlyList<VendorRateModel> __rates, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM VendorRates WHERE VendorId = @VendorId",
            new { VendorId = __vendorId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO VendorRates (Id, VendorId, DestinationId, ServiceDescription, RateAmount, Currency, ValidFrom, ValidTo, CreatedAt, IsDeleted)
            VALUES (@Id, @VendorId, @DestinationId, @ServiceDescription, @RateAmount, @Currency, @ValidFrom, @ValidTo, SYSUTCDATETIME(), 0)
            """;
        foreach (var rate in __rates)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                VendorId = __vendorId,
                rate.DestinationId,
                rate.ServiceDescription,
                rate.RateAmount,
                rate.Currency,
                rate.ValidFrom,
                rate.ValidTo
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<PaginationResponse<VendorPaymentModel>> GetPaymentsAsync(Guid __vendorId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var sql = $"""
            {PaymentSelectColumns}
            WHERE vp.VendorId = @VendorId AND vp.IsDeleted = 0
            ORDER BY vp.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM VendorPayments vp WHERE vp.VendorId = @VendorId AND vp.IsDeleted = 0;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { VendorId = __vendorId, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<VendorPaymentModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<VendorPaymentModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<VendorPaymentModel?> GetPaymentByIdAsync(Guid __paymentId, CancellationToken __cancellationToken)
    {
        var sql = $"{PaymentSelectColumns} WHERE vp.Id = @Id AND vp.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<VendorPaymentModel>(
            new CommandDefinition(sql, new { Id = __paymentId }, cancellationToken: __cancellationToken));
    }

    public async Task CreatePaymentAsync(VendorPaymentModel __payment, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO VendorPayments (Id, VendorId, BookingId, Amount, Status, Notes, PaidAt, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @VendorId, @BookingId, @Amount, @Status, @Notes, @PaidAt, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __payment, cancellationToken: __cancellationToken));
    }

    public async Task UpdatePaymentStatusAsync(Guid __paymentId, string __status, DateTime? __paidAt, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE VendorPayments
            SET Status = @Status, PaidAt = COALESCE(@PaidAt, PaidAt), UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __paymentId, Status = __status, PaidAt = __paidAt, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<VendorPerformanceModel>> GetPerformanceAsync(Guid __vendorId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT vp.Id, vp.VendorId, vp.BookingId, b.BookingNumber, vp.Rating, vp.Notes, vp.RecordedBy, u.FullName AS RecordedByName,
                   vp.CreatedAt, vp.CreatedBy, vp.UpdatedAt, vp.UpdatedBy, vp.IsDeleted
            FROM VendorPerformance vp
            LEFT JOIN Bookings b ON b.Id = vp.BookingId
            LEFT JOIN Users u ON u.Id = vp.RecordedBy
            WHERE vp.VendorId = @VendorId AND vp.IsDeleted = 0
            ORDER BY vp.CreatedAt DESC
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<VendorPerformanceModel>(
            new CommandDefinition(sql, new { VendorId = __vendorId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task CreatePerformanceAsync(VendorPerformanceModel __performance, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO VendorPerformance (Id, VendorId, BookingId, Rating, Notes, RecordedBy, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @VendorId, @BookingId, @Rating, @Notes, @RecordedBy, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __performance, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<VendorBookingRequestModel>> GetBookingRequestsAsync(Guid __vendorId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT b.Id AS BookingId, b.BookingNumber, b.Status, b.TravelDate, b.ReturnDate,
                   b.NumberOfAdults, b.NumberOfChildren, b.DestinationId, d.Name AS DestinationName, b.CreatedAt
            FROM Bookings b
            INNER JOIN VendorRates vr ON vr.DestinationId = b.DestinationId AND vr.VendorId = @VendorId AND vr.IsDeleted = 0
            LEFT JOIN Destinations d ON d.Id = b.DestinationId
            WHERE b.IsDeleted = 0
            ORDER BY b.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(DISTINCT b.Id)
            FROM Bookings b
            INNER JOIN VendorRates vr ON vr.DestinationId = b.DestinationId AND vr.VendorId = @VendorId AND vr.IsDeleted = 0
            WHERE b.IsDeleted = 0;
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { VendorId = __vendorId, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<VendorBookingRequestModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<VendorBookingRequestModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }
}
