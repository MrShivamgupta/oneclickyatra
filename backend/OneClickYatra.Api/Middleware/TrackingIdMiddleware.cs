using Serilog.Context;

namespace OneClickYatra.Api.Middleware;

/// <summary>
/// Assigns a Tracking ID to every request: preserves the client-sent "X-Tracking-Id" header if
/// present, otherwise generates one. The id is pushed onto the Serilog LogContext so every log
/// line for this request carries it, stored on HttpContext.Items for downstream code, and echoed
/// back on the response header.
/// </summary>
public sealed class TrackingIdMiddleware
{
    public const string HeaderName = "X-Tracking-Id";
    public const string ItemsKey = "TrackingId";

    private readonly RequestDelegate _next;

    public TrackingIdMiddleware(RequestDelegate __next)
    {
        _next = __next;
    }

    public async Task InvokeAsync(HttpContext __context)
    {
        var trackingId = __context.Request.Headers.TryGetValue(HeaderName, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString("N");

        __context.Items[ItemsKey] = trackingId;
        __context.Response.Headers[HeaderName] = trackingId;

        using (LogContext.PushProperty("TrackingId", trackingId))
        {
            await _next(__context);
        }
    }
}
