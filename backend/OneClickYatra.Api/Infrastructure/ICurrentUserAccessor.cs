using System.Security.Claims;
using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Infrastructure;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }

    /// <summary>SuperAdmin bypasses per-record ownership checks (e.g. lead assignment) the same way
    /// it already bypasses everything on the frontend via TokenService.hasPermission — a single,
    /// consistent "SuperAdmin can always act" rule rather than a second one invented here.</summary>
    bool IsSuperAdmin { get; }
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

    public bool IsSuperAdmin => _httpContextAccessor.HttpContext?.User.IsInRole(RoleConstants.SuperAdmin) ?? false;
}
