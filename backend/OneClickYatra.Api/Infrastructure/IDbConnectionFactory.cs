using System.Data;

namespace OneClickYatra.Api.Infrastructure;

/// <summary>Every repository resolves its SQL connection through here — the single place that knows the connection string.</summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
