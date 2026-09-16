using Microsoft.AspNetCore.Authorization;
using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Security.Authorization;

/// <summary>
/// SuperAdmin always passes. Everyone else must carry a "permission" claim matching the
/// requirement (claims are issued at login time from RolePermissions, see JwtTokenService).
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext __context,
        PermissionRequirement __requirement)
    {
        if (__context.User.IsInRole(RoleConstants.SuperAdmin))
        {
            __context.Succeed(__requirement);
            return Task.CompletedTask;
        }

        var hasPermission = __context.User.Claims.Any(claim =>
            claim.Type == "permission" && claim.Value == __requirement.Permission);

        if (hasPermission)
        {
            __context.Succeed(__requirement);
        }

        return Task.CompletedTask;
    }
}
