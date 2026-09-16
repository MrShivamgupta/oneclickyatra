namespace OneClickYatra.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "OneClickYatraCors";

    public static IServiceCollection AddAppCors(this IServiceCollection __services, IConfiguration __configuration)
    {
        var allowedOrigins = __configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        __services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return __services;
    }
}
