using Serilog;

namespace OneClickYatra.Api.Extensions;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddAppSerilog(this WebApplicationBuilder __builder)
    {
        __builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30);
        });

        return __builder;
    }
}
