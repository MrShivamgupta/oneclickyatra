using OneClickYatra.Api.HealthChecks;

namespace OneClickYatra.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection __services, IConfiguration __configuration)
    {
        __services.AddHealthChecks()
            .AddCheck<ApplicationHealthCheck>("application", tags: ["live"])
            .AddSqlServer(
                __configuration.GetConnectionString("SqlServer") ?? string.Empty,
                name: "sql-server",
                tags: ["ready"])
            .AddRedis(
                __configuration.GetConnectionString("Redis") ?? "localhost:6379",
                name: "redis",
                tags: ["ready"]);

        return __services;
    }
}
