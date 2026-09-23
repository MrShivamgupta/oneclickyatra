using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IRoleAppFunction
{
    Task<PermissionMatrixResponse> GetPermissionMatrixAsync(CancellationToken __cancellationToken);
    Task<PermissionMatrixResponse> UpdateRolePermissionsAsync(Guid __roleId, UpdateRolePermissionsRequest __request, CancellationToken __cancellationToken);
}
