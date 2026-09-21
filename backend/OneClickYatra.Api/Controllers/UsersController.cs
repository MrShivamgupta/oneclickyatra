using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserAppFunction _userAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public UsersController(IUserAppFunction __userAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _userAppFunction = __userAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Staff (non-Customer/Guest) users, for pickers such as "Assign lead to".</summary>
    [HttpGet("staff")]
    [HasPermission(PermissionConstants.LeadView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserSummaryResponse>>>> ListStaff(CancellationToken __cancellationToken)
    {
        var result = await _userAppFunction.ListStaffAsync(__cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserSummaryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Searches staff (TravelAgent/OperationsStaff/Finance/SuperAdmin) users for the admin
    /// "Users" management screen, with optional name/email search, role filter and active-status filter.</summary>
    [HttpGet]
    [HasPermission(PermissionConstants.UserManage)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginationResponse<UserResponse>>))]
    public async Task<ActionResult<ApiResponse<PaginationResponse<UserResponse>>>> Search([FromQuery] UserSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _userAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<UserResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Creates a new staff account. Role must be TravelAgent/OperationsStaff/Finance/SuperAdmin
    /// — Customer/Guest/Vendor accounts are created through their own existing flows.</summary>
    [HttpPost]
    [HasPermission(PermissionConstants.UserManage)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Create(CreateStaffUserRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _userAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "User created."));
    }

    /// <summary>Changes a staff user's role: their current staff role(s) are removed and the new one assigned.</summary>
    [HttpPut("{id:guid}/role")]
    [HasPermission(PermissionConstants.UserManage)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateRole(Guid id, UpdateUserRoleRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _userAppFunction.UpdateRoleAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "User role updated."));
    }

    /// <summary>Activates or deactivates a staff user. There is no hard delete for users.</summary>
    [HttpPut("{id:guid}/status")]
    [HasPermission(PermissionConstants.UserManage)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateStatus(Guid id, UpdateUserStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _userAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "User status updated."));
    }
}
