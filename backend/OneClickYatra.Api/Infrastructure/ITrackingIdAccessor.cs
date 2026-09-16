using OneClickYatra.Api.Middleware;

namespace OneClickYatra.Api.Infrastructure;

/// <summary>Exposes the current request's Tracking ID to AppFunctions/Repositories for logging and audit rows.</summary>
public interface ITrackingIdAccessor
{
    string TrackingId { get; }
}

public sealed class HttpContextTrackingIdAccessor : ITrackingIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTrackingIdAccessor(IHttpContextAccessor __httpContextAccessor)
    {
        _httpContextAccessor = __httpContextAccessor;
    }

    public string TrackingId =>
        _httpContextAccessor.HttpContext?.Items[TrackingIdMiddleware.ItemsKey] as string
        ?? Guid.NewGuid().ToString("N");
}
