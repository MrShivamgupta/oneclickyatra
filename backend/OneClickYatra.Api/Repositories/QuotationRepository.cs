using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public sealed class QuotationRepository : IQuotationRepository
{
    private const string SelectColumns = """
        SELECT q.Id, q.QuotationNumber, q.LeadId, q.CustomerId, q.Title, q.Status, q.ValidUntil, q.Notes,
               q.PublicTokenHash, q.SelectedOptionId, q.ApprovedAt, q.ApprovedByName, q.RejectionReason,
               q.CreatedAt, q.CreatedBy, q.UpdatedAt, q.UpdatedBy, q.IsDeleted,
               l.CustomerName AS LeadCustomerName, c.FullName AS CustomerName
        FROM Quotations q
        INNER JOIN Leads l ON l.Id = q.LeadId
        LEFT JOIN Customers c ON c.Id = q.CustomerId
        """;

    private const string OptionSelectColumns = """
        SELECT o.Id, o.QuotationId, o.PackageId, o.OptionName, o.DestinationId, o.DurationDays, o.DurationNights,
               o.HotelCategory, o.NumberOfPeople, o.PricePerPerson, o.TotalPrice, o.IsRecommended, o.SortOrder,
               d.Name AS DestinationName
        FROM QuotationOptions o
        LEFT JOIN Destinations d ON d.Id = o.DestinationId
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public QuotationRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<QuotationModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE q.Id = @Id AND q.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<QuotationModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<QuotationModel?> GetByPublicTokenHashAsync(string __publicTokenHash, CancellationToken __cancellationToken)
    {
        var sql = $"{SelectColumns} WHERE q.PublicTokenHash = @PublicTokenHash AND q.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<QuotationModel>(
            new CommandDefinition(sql, new { PublicTokenHash = __publicTokenHash }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<QuotationModel>> SearchAsync(QuotationSearchRequest __request, CancellationToken __cancellationToken)
    {
        const string whereClause = """
            WHERE q.IsDeleted = 0
              AND (@Status IS NULL OR q.Status = @Status)
              AND (@LeadId IS NULL OR q.LeadId = @LeadId)
              AND (@SearchTerm IS NULL OR q.QuotationNumber LIKE '%' + @SearchTerm + '%' OR q.Title LIKE '%' + @SearchTerm + '%' OR l.CustomerName LIKE '%' + @SearchTerm + '%')
            """;

        var sql = $"""
            {SelectColumns}
            {whereClause}
            ORDER BY q.CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM Quotations q
            INNER JOIN Leads l ON l.Id = q.LeadId
            {whereClause};
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __request.Status,
            __request.LeadId,
            __request.SearchTerm,
            __request.Skip,
            __request.PageSize
        }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<QuotationModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<QuotationModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task CreateAsync(QuotationModel __quotation, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Quotations
                (Id, QuotationNumber, LeadId, CustomerId, Title, Status, ValidUntil, Notes, PublicTokenHash, CreatedAt, CreatedBy, IsDeleted)
            VALUES
                (@Id, @QuotationNumber, @LeadId, @CustomerId, @Title, @Status, @ValidUntil, @Notes, @PublicTokenHash, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __quotation, cancellationToken: __cancellationToken));
    }

    public async Task UpdateAsync(QuotationModel __quotation, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Quotations
            SET Title = @Title, ValidUntil = @ValidUntil, Notes = @Notes,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __quotation, cancellationToken: __cancellationToken));
    }

    public async Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Quotations SET Status = @Status, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Status = __status, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task ApproveAsync(Guid __id, Guid __selectedOptionId, string __approvedByName, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Quotations
            SET Status = 'Approved', SelectedOptionId = @SelectedOptionId, ApprovedAt = SYSUTCDATETIME(),
                ApprovedByName = @ApprovedByName, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, SelectedOptionId = __selectedOptionId, ApprovedByName = __approvedByName }, cancellationToken: __cancellationToken));
    }

    public async Task RejectAsync(Guid __id, string? __reason, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Quotations SET Status = 'Rejected', RejectionReason = @Reason, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, Reason = __reason }, cancellationToken: __cancellationToken));
    }

    public async Task UpdatePublicTokenHashAsync(Guid __id, string __publicTokenHash, Guid? __updatedBy, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Quotations SET PublicTokenHash = @PublicTokenHash, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, PublicTokenHash = __publicTokenHash, UpdatedBy = __updatedBy }, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Quotations SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<QuotationOptionModel>> GetOptionsAsync(Guid __quotationId, CancellationToken __cancellationToken)
    {
        var sql = $"{OptionSelectColumns} WHERE o.QuotationId = @QuotationId AND o.IsDeleted = 0 ORDER BY o.SortOrder";
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<QuotationOptionModel>(
            new CommandDefinition(sql, new { QuotationId = __quotationId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<QuotationOptionModel?> GetOptionByIdAsync(Guid __optionId, CancellationToken __cancellationToken)
    {
        var sql = $"{OptionSelectColumns} WHERE o.Id = @Id AND o.IsDeleted = 0";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<QuotationOptionModel>(
            new CommandDefinition(sql, new { Id = __optionId }, cancellationToken: __cancellationToken));
    }

    public async Task<IReadOnlyList<QuotationItemModel>> GetItemsForOptionsAsync(IReadOnlyList<Guid> __optionIds, CancellationToken __cancellationToken)
    {
        if (__optionIds.Count == 0)
        {
            return [];
        }

        const string sql = """
            SELECT Id, QuotationOptionId, Description, Category, Amount, SortOrder
            FROM QuotationItems
            WHERE QuotationOptionId IN @OptionIds AND IsDeleted = 0
            ORDER BY SortOrder
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<QuotationItemModel>(
            new CommandDefinition(sql, new { OptionIds = __optionIds }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceOptionsAsync(Guid __quotationId, IReadOnlyList<(QuotationOptionModel Option, List<QuotationItemModel> Items)> __options, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // Clear the parent's pointer first — the old options are about to be deleted and
        // SelectedOptionId has an FK to QuotationOptions.
        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE Quotations SET SelectedOptionId = NULL WHERE Id = @QuotationId",
            new { QuotationId = __quotationId }, transaction, cancellationToken: __cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            """
            DELETE FROM QuotationItems WHERE QuotationOptionId IN (
                SELECT Id FROM QuotationOptions WHERE QuotationId = @QuotationId
            )
            """,
            new { QuotationId = __quotationId }, transaction, cancellationToken: __cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM QuotationOptions WHERE QuotationId = @QuotationId",
            new { QuotationId = __quotationId }, transaction, cancellationToken: __cancellationToken));

        const string insertOptionSql = """
            INSERT INTO QuotationOptions
                (Id, QuotationId, PackageId, OptionName, DestinationId, DurationDays, DurationNights,
                 HotelCategory, NumberOfPeople, PricePerPerson, TotalPrice, IsRecommended, SortOrder, CreatedAt, IsDeleted)
            VALUES
                (@Id, @QuotationId, @PackageId, @OptionName, @DestinationId, @DurationDays, @DurationNights,
                 @HotelCategory, @NumberOfPeople, @PricePerPerson, @TotalPrice, @IsRecommended, @SortOrder, SYSUTCDATETIME(), 0)
            """;

        const string insertItemSql = """
            INSERT INTO QuotationItems (Id, QuotationOptionId, Description, Category, Amount, SortOrder, CreatedAt, IsDeleted)
            VALUES (@Id, @QuotationOptionId, @Description, @Category, @Amount, @SortOrder, SYSUTCDATETIME(), 0)
            """;

        foreach (var (option, items) in __options)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertOptionSql, new
            {
                option.Id,
                QuotationId = __quotationId,
                option.PackageId,
                option.OptionName,
                option.DestinationId,
                option.DurationDays,
                option.DurationNights,
                option.HotelCategory,
                option.NumberOfPeople,
                option.PricePerPerson,
                option.TotalPrice,
                option.IsRecommended,
                option.SortOrder
            }, transaction, cancellationToken: __cancellationToken));

            foreach (var item in items)
            {
                await connection.ExecuteAsync(new CommandDefinition(insertItemSql, new
                {
                    item.Id,
                    QuotationOptionId = option.Id,
                    item.Description,
                    item.Category,
                    item.Amount,
                    item.SortOrder
                }, transaction, cancellationToken: __cancellationToken));
            }
        }

        transaction.Commit();
    }

    public async Task CreateApprovalAsync(QuotationApprovalModel __approval, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO QuotationApprovals
                (Id, QuotationId, SelectedOptionId, Decision, ApprovedByName, Comments, IpAddress, DecidedAt, CreatedAt, IsDeleted)
            VALUES
                (@Id, @QuotationId, @SelectedOptionId, @Decision, @ApprovedByName, @Comments, @IpAddress, SYSUTCDATETIME(), SYSUTCDATETIME(), 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __approval, cancellationToken: __cancellationToken));
    }
}
