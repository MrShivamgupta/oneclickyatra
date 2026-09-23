using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class AgencyProfileRepository : IAgencyProfileRepository
{
    private const string SelectColumns = """
        SELECT TOP (1) Id, Name, LogoUrl, Address, GstNumber, Currency, SupportEmail, SupportPhone, UpdatedAt
        FROM AgencyProfile
        WHERE IsDeleted = 0
        ORDER BY CreatedAt ASC
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public AgencyProfileRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<AgencyProfileModel?> GetAsync(CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<AgencyProfileModel>(
            new CommandDefinition(SelectColumns, cancellationToken: __cancellationToken));
    }

    public async Task UpdateAsync(AgencyProfileModel __profile, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE AgencyProfile
            SET Name = @Name, LogoUrl = @LogoUrl, Address = @Address, GstNumber = @GstNumber,
                Currency = @Currency, SupportEmail = @SupportEmail, SupportPhone = @SupportPhone,
                UpdatedAt = SYSUTCDATETIME(), UpdatedBy = @UpdatedBy
            WHERE Id = @Id AND IsDeleted = 0
            """;
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, __profile, cancellationToken: __cancellationToken));
    }
}
