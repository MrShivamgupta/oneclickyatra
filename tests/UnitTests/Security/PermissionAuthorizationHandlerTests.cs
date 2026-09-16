using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.UnitTests.Security;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler = new();

    private static AuthorizationHandlerContext CreateContext(PermissionRequirement requirement, ClaimsPrincipal user) =>
        new([requirement], user, null);

    [Fact]
    public async Task SuperAdmin_AlwaysSucceeds_RegardlessOfPermissionClaims()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, RoleConstants.SuperAdmin)], "TestAuth"));
        var context = CreateContext(new PermissionRequirement(PermissionConstants.UserManage), user);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task UserWithMatchingPermissionClaim_Succeeds()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permission", PermissionConstants.LeadView)], "TestAuth"));
        var context = CreateContext(new PermissionRequirement(PermissionConstants.LeadView), user);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task UserWithoutMatchingPermissionClaim_DoesNotSucceed()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("permission", PermissionConstants.LeadView)], "TestAuth"));
        var context = CreateContext(new PermissionRequirement(PermissionConstants.UserManage), user);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
