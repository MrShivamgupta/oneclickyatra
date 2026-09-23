using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// Backs the "Roles &amp; Permissions" admin page. Deliberately narrow: this only lets a SuperAdmin
/// reconfigure which permission keys a role holds (RolePermissions), the same table
/// [HasPermission] already checks against at request time everywhere else in the API. It does not
/// add a second, independently-maintained authorization system — a role's page-level permissions
/// here ARE its API-level permissions.
/// </summary>
public sealed class RoleAppFunction : IRoleAppFunction
{
    private readonly IRoleRepository _roleRepository;

    public RoleAppFunction(IRoleRepository __roleRepository)
    {
        _roleRepository = __roleRepository;
    }

    public async Task<PermissionMatrixResponse> GetPermissionMatrixAsync(CancellationToken __cancellationToken)
    {
        var roles = await _roleRepository.GetAllRolesAsync(__cancellationToken);
        var permissions = await _roleRepository.GetAllPermissionsAsync(__cancellationToken);
        var grants = await _roleRepository.GetAllGrantsAsync(__cancellationToken);

        var grantsByRole = grants
            .GroupBy(g => g.RoleId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.PermissionKey).OrderBy(k => k).ToList());

        return new PermissionMatrixResponse
        {
            Permissions = permissions.Select(p => new PermissionResponse { Key = p.Key, Description = p.Description }).ToList(),
            Roles = roles.Select(r => new RolePermissionsResponse
            {
                RoleId = r.Id,
                RoleName = r.Name,
                Description = r.Description,
                PermissionKeys = grantsByRole.GetValueOrDefault(r.Id, Array.Empty<string>())
            }).ToList()
        };
    }

    public async Task<PermissionMatrixResponse> UpdateRolePermissionsAsync(Guid __roleId, UpdateRolePermissionsRequest __request, CancellationToken __cancellationToken)
    {
        var role = await _roleRepository.GetRoleByIdAsync(__roleId, __cancellationToken) ?? throw new EntityNotFoundException("Role", __roleId);

        var requestedKeys = __request.PermissionKeys.Distinct().ToList();
        var resolvedPermissions = await _roleRepository.GetPermissionsByKeysAsync(requestedKeys, __cancellationToken);

        if (resolvedPermissions.Count != requestedKeys.Count)
        {
            var unknownKeys = requestedKeys.Except(resolvedPermissions.Select(p => p.Key));
            throw new BusinessException($"Unknown permission key(s): {string.Join(", ", unknownKeys)}");
        }

        // Safety rail against a self-inflicted lockout: without role.manage on SOME role, nobody
        // could ever open this page again to fix a mistake here, and SuperAdmin is the only role
        // ever granted it. Not a general access-control decision, just guarding this one page.
        if (role.Name == RoleConstants.SuperAdmin && !resolvedPermissions.Any(p => p.Key == PermissionConstants.RoleManage))
        {
            throw new BusinessException("Cannot remove the Role Management permission from SuperAdmin — doing so would lock every administrator out of this page.");
        }

        await _roleRepository.ReplaceRolePermissionsAsync(__roleId, resolvedPermissions.Select(p => p.Id).ToList(), __cancellationToken);

        return await GetPermissionMatrixAsync(__cancellationToken);
    }
}
