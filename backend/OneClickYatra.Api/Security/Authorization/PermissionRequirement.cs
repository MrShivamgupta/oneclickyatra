using Microsoft.AspNetCore.Authorization;

namespace OneClickYatra.Api.Security.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string __permission)
    {
        Permission = __permission;
    }
}
