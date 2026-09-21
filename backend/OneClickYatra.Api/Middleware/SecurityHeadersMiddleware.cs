namespace OneClickYatra.Api.Middleware;

/// <summary>
/// Sets the security-relevant HTTP response headers ASP.NET Core does not set by default.
/// Everywhere except the self-hosted Swagger UI / Hangfire Dashboard pages gets a strict,
/// no-script/no-style CSP (this is a pure JSON API — nothing here should ever need to load a
/// script or stylesheet); those two paths still get the frame/sniff/referrer protections but a
/// relaxed CSP that allows their own self-hosted assets, or their UI simply won't render.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private const string StrictCsp = "default-src 'none'; frame-ancestors 'none'";
    private const string RelaxedCsp = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate __next)
    {
        _next = __next;
    }

    public async Task InvokeAsync(HttpContext __context)
    {
        var path = __context.Request.Path.Value ?? string.Empty;
        var isSelfHostedUiPage = path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase);

        __context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        __context.Response.Headers["X-Frame-Options"] = "DENY";
        __context.Response.Headers["Referrer-Policy"] = "no-referrer";
        __context.Response.Headers["Content-Security-Policy"] = isSelfHostedUiPage ? RelaxedCsp : StrictCsp;

        // Note: the "Server: Kestrel" header is NOT removable here — Kestrel adds it at the
        // transport layer, after middleware has run. It's disabled at the source instead, via
        // options.AddServerHeader = false in Program.cs's WebHost configuration.
        await _next(__context);
    }
}
