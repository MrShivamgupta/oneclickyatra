using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IRoleRepository
{
    Task<IReadOnlyList<RoleModel>> GetAllRolesAsync(CancellationToken __cancellationToken);
    Task<IReadOnlyList<PermissionModel>> GetAllPermissionsAsync(CancellationToken __cancellationToken);

    /// <summary>Every (RoleId, PermissionKey) pair currently granted, across all roles.</summary>
    Task<IReadOnlyList<(Guid RoleId, string PermissionKey)>> GetAllGrantsAsync(CancellationToken __cancellationToken);

    Task<RoleModel?> GetRoleByIdAsync(Guid __roleId, CancellationToken __cancellationToken);

    /// <summary>Resolves permission keys to Ids, skipping any key that doesn't exist. Used to validate
    /// a requested permission set before replacing a role's grants.</summary>
    Task<IReadOnlyList<PermissionModel>> GetPermissionsByKeysAsync(IReadOnlyList<string> __keys, CancellationToken __cancellationToken);

    /// <summary>Full replace: deletes every existing grant for the role, then inserts the given set,
    /// in one transaction — mirrors BookingRepository's ReplacePassengersAsync/ReplaceAddOnsAsync.</summary>
    Task ReplaceRolePermissionsAsync(Guid __roleId, IReadOnlyList<Guid> __permissionIds, CancellationToken __cancellationToken);
}
