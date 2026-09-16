using System.Diagnostics;

namespace OneClickYatra.Api.Middleware;

/// <summary>Logs method, path, status code and elapsed time for every request (correlated via TrackingId).</summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate __next, ILogger<RequestLoggingMiddleware> __logger)
    {
        _next = __next;
        _logger = __logger;
    }

    public async Task InvokeAsync(HttpContext __context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(__context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                __context.Request.Method,
                __context.Request.Path,
                __context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
