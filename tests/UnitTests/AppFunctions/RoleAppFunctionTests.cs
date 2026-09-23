using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.AppFunctions;

public class RoleAppFunctionTests
{
    private readonly Mock<IRoleRepository> _roleRepository = new();

    private RoleAppFunction CreateSut() => new(_roleRepository.Object);

    private static RoleModel MakeRole(string name) => new() { Id = Guid.NewGuid(), Name = name, Description = null };
    private static PermissionModel MakePermission(string key) => new() { Id = Guid.NewGuid(), Key = key, Description = null };

    [Fact]
    public async Task GetPermissionMatrixAsync_GroupsGrantsByRole()
    {
        var superAdmin = MakeRole(RoleConstants.SuperAdmin);
        var travelAgent = MakeRole(RoleConstants.TravelAgent);
        _roleRepository.Setup(r => r.GetAllRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { superAdmin, travelAgent });
        _roleRepository.Setup(r => r.GetAllPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakePermission(PermissionConstants.LeadView), MakePermission(PermissionConstants.RoleManage) });
        _roleRepository.Setup(r => r.GetAllGrantsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[]
        {
            (superAdmin.Id, PermissionConstants.LeadView),
            (superAdmin.Id, PermissionConstants.RoleManage),
            (travelAgent.Id, PermissionConstants.LeadView)
        });

        var sut = CreateSut();
        var result = await sut.GetPermissionMatrixAsync(CancellationToken.None);

        Assert.Equal(2, result.Permissions.Count);
        var superAdminRow = result.Roles.Single(r => r.RoleId == superAdmin.Id);
        var travelAgentRow = result.Roles.Single(r => r.RoleId == travelAgent.Id);
        Assert.Equal(new[] { PermissionConstants.LeadView, PermissionConstants.RoleManage }, superAdminRow.PermissionKeys);
        Assert.Equal(new[] { PermissionConstants.LeadView }, travelAgentRow.PermissionKeys);
    }

    [Fact]
    public async Task GetPermissionMatrixAsync_RoleWithNoGrants_ReturnsEmptyPermissionKeys()
    {
        var vendor = MakeRole(RoleConstants.Vendor);
        _roleRepository.Setup(r => r.GetAllRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { vendor });
        _roleRepository.Setup(r => r.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<PermissionModel>());
        _roleRepository.Setup(r => r.GetAllGrantsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<(Guid, string)>());

        var sut = CreateSut();
        var result = await sut.GetPermissionMatrixAsync(CancellationToken.None);

        Assert.Empty(Assert.Single(result.Roles).PermissionKeys);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_RoleDoesNotExist_ThrowsEntityNotFound()
    {
        _roleRepository.Setup(r => r.GetRoleByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoleModel?)null);

        var sut = CreateSut();
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.UpdateRolePermissionsAsync(Guid.NewGuid(), new UpdateRolePermissionsRequest { PermissionKeys = [PermissionConstants.LeadView] }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_UnknownPermissionKey_ThrowsBusinessException()
    {
        var role = MakeRole(RoleConstants.TravelAgent);
        _roleRepository.Setup(r => r.GetRoleByIdAsync(role.Id, It.IsAny<CancellationToken>())).ReturnsAsync(role);
        _roleRepository.Setup(r => r.GetPermissionsByKeysAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PermissionModel>()); // simulates "not.a.real.key" resolving to nothing

        var sut = CreateSut();
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            sut.UpdateRolePermissionsAsync(role.Id, new UpdateRolePermissionsRequest { PermissionKeys = ["not.a.real.key"] }, CancellationToken.None));

        Assert.Contains("not.a.real.key", ex.Message);
        _roleRepository.Verify(r => r.ReplaceRolePermissionsAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_RemovingRoleManageFromSuperAdmin_ThrowsBusinessException()
    {
        var superAdmin = MakeRole(RoleConstants.SuperAdmin);
        var leadView = MakePermission(PermissionConstants.LeadView);
        _roleRepository.Setup(r => r.GetRoleByIdAsync(superAdmin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(superAdmin);
        _roleRepository.Setup(r => r.GetPermissionsByKeysAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { leadView }); // request omits role.manage entirely

        var sut = CreateSut();
        await Assert.ThrowsAsync<BusinessException>(() =>
            sut.UpdateRolePermissionsAsync(superAdmin.Id, new UpdateRolePermissionsRequest { PermissionKeys = [PermissionConstants.LeadView] }, CancellationToken.None));

        _roleRepository.Verify(r => r.ReplaceRolePermissionsAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_ValidRequestForNonSuperAdminRole_ReplacesAndReturnsMatrix()
    {
        var travelAgent = MakeRole(RoleConstants.TravelAgent);
        var leadView = MakePermission(PermissionConstants.LeadView);
        _roleRepository.Setup(r => r.GetRoleByIdAsync(travelAgent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(travelAgent);
        _roleRepository.Setup(r => r.GetPermissionsByKeysAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { leadView });
        _roleRepository.Setup(r => r.GetAllRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { travelAgent });
        _roleRepository.Setup(r => r.GetAllPermissionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { leadView });
        _roleRepository.Setup(r => r.GetAllGrantsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { (travelAgent.Id, PermissionConstants.LeadView) });

        var sut = CreateSut();
        var result = await sut.UpdateRolePermissionsAsync(travelAgent.Id, new UpdateRolePermissionsRequest { PermissionKeys = [PermissionConstants.LeadView] }, CancellationToken.None);

        _roleRepository.Verify(r => r.ReplaceRolePermissionsAsync(travelAgent.Id, new[] { leadView.Id }, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(new[] { PermissionConstants.LeadView }, Assert.Single(result.Roles).PermissionKeys);
    }
}
