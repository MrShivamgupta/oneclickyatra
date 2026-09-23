using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

/// <summary>SuperAdmin-only screen for reconfiguring which permissions each role holds.</summary>
[ApiController]
[Route("api/v1/roles")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleAppFunction _roleAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public RolesController(IRoleAppFunction __roleAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _roleAppFunction = __roleAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet("permissions-matrix")]
    [HasPermission(PermissionConstants.RoleManage)]
    public async Task<ActionResult<ApiResponse<PermissionMatrixResponse>>> GetPermissionMatrix(CancellationToken __cancellationToken)
    {
        var result = await _roleAppFunction.GetPermissionMatrixAsync(__cancellationToken);
        return Ok(ApiResponse<PermissionMatrixResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("{id:guid}/permissions")]
    [HasPermission(PermissionConstants.RoleManage)]
    public async Task<ActionResult<ApiResponse<PermissionMatrixResponse>>> UpdateRolePermissions(
        Guid id, [FromBody] UpdateRolePermissionsRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _roleAppFunction.UpdateRolePermissionsAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<PermissionMatrixResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Permissions updated successfully"));
    }
}
