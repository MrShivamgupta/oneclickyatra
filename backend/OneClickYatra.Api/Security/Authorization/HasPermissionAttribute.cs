using Microsoft.AspNetCore.Authorization;

namespace OneClickYatra.Api.Security.Authorization;

/// <summary>
/// Usage: [HasPermission(PermissionConstants.BookingCreate)] on a controller action.
/// Combines the standard [Authorize] pipeline with a permission-claim requirement, so both
/// "must be authenticated" and "must hold this specific permission" are enforced together.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    public string Permission { get; }

    public HasPermissionAttribute(string __permission)
    {
        Permission = __permission;
    }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new PermissionRequirement(Permission);
    }
}
