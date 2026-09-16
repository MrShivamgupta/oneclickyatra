using Hangfire;
using Hangfire.SqlServer;

namespace OneClickYatra.Api.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddAppHangfire(this IServiceCollection __services, IConfiguration __configuration)
    {
        var connectionString = __configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("ConnectionStrings:SqlServer is not configured.");

        __services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        __services.AddHangfireServer();

        return __services;
    }
}
