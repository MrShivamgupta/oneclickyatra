using Dapper;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CustomerRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<CustomerModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, FullName, Email, Phone, UserId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Customers WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CustomerModel>(
            new CommandDefinition(sql, new { Id = __id }, cancellationToken: __cancellationToken));
    }

    public async Task<CustomerModel?> GetByPhoneAsync(string __phone, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, FullName, Email, Phone, UserId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Customers WHERE Phone = @Phone AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<CustomerModel>(
            new CommandDefinition(sql, new { Phone = __phone }, cancellationToken: __cancellationToken));
    }

    public async Task<PaginationResponse<CustomerModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, FullName, Email, Phone, UserId, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Customers
            WHERE IsDeleted = 0
              AND (@SearchTerm IS NULL OR FullName LIKE '%' + @SearchTerm + '%' OR Phone LIKE '%' + @SearchTerm + '%' OR Email LIKE '%' + @SearchTerm + '%')
            ORDER BY CreatedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) FROM Customers
            WHERE IsDeleted = 0
              AND (@SearchTerm IS NULL OR FullName LIKE '%' + @SearchTerm + '%' OR Phone LIKE '%' + @SearchTerm + '%' OR Email LIKE '%' + @SearchTerm + '%');
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { __request.SearchTerm, __request.Skip, __request.PageSize }, cancellationToken: __cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var items = (await multi.ReadAsync<CustomerModel>()).ToList();
        var total = await multi.ReadSingleAsync<long>();

        return PaginationResponse<CustomerModel>.Create(items, __request.PageNumber, __request.PageSize, total);
    }

    public async Task<Guid> CreateAsync(CustomerModel __customer, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Customers (Id, FullName, Email, Phone, UserId, CreatedAt, CreatedBy, IsDeleted)
            VALUES (@Id, @FullName, @Email, @Phone, @UserId, SYSUTCDATETIME(), @CreatedBy, 0)
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __customer, cancellationToken: __cancellationToken));
        return __customer.Id;
    }

    public async Task UpdateAsync(CustomerModel __customer, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Customers
            SET FullName = @FullName, Email = @Email, Phone = @Phone, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __customer, cancellationToken: __cancellationToken));
    }

    public async Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken)
    {
        const string sql = "UPDATE Customers SET IsDeleted = 1, UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @DeletedBy WHERE Id = @Id";
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = __id, DeletedBy = __deletedBy }, cancellationToken: __cancellationToken));
    }
}
