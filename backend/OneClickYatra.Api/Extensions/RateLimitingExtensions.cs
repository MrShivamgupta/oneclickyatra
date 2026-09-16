using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;

namespace OneClickYatra.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection __services, IConfiguration __configuration)
    {
        var publicPermitLimit = __configuration.GetValue("RateLimiting:PublicPermitLimit", 20);
        var publicWindowSeconds = __configuration.GetValue("RateLimiting:PublicWindowSeconds", 60);
        var authPermitLimit = __configuration.GetValue("RateLimiting:AuthPermitLimit", 5);
        var authWindowSeconds = __configuration.GetValue("RateLimiting:AuthWindowSeconds", 60);

        __services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, __cancellationToken) =>
            {
                var trackingIdAccessor = context.HttpContext.RequestServices.GetRequiredService<ITrackingIdAccessor>();
                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
                var envelope = ApiResponse<object?>.Fail(
                    trackingIdAccessor.TrackingId,
                    "Too many requests. Please wait a minute and try again.");
                await context.HttpContext.Response.WriteAsJsonAsync(envelope, __cancellationToken);
            };

            options.AddPolicy("public", context => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = publicPermitLimit,
                    Window = TimeSpan.FromSeconds(publicWindowSeconds),
                    QueueLimit = 0
                }));

            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authPermitLimit,
                    Window = TimeSpan.FromSeconds(authWindowSeconds),
                    QueueLimit = 0
                }));
        });

        return __services;
    }
}
