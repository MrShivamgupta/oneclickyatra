using System.Data;
using Microsoft.Data.SqlClient;
using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Infrastructure;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration __configuration)
    {
        _connectionString = __configuration.GetConnectionString("SqlServer")
            ?? throw new ConfigurationException("The service is temporarily unavailable. Please try again shortly.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
