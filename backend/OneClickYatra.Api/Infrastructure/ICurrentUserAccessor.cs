using System.Security.Claims;

namespace OneClickYatra.Api.Infrastructure;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}

public sealed class HttpContextCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserAccessor(IHttpContextAccessor __httpContextAccessor)
    {
        _httpContextAccessor = __httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
