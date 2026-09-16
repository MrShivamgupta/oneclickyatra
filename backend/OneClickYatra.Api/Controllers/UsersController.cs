using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
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
}
