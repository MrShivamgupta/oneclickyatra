using System.Data;
using Microsoft.Data.SqlClient;

namespace OneClickYatra.Api.Infrastructure;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration __configuration)
    {
        _connectionString = __configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is not configured.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
