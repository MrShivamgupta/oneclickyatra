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
[Route("api/v1/agency-profile")]
[Authorize]
public sealed class AgencyProfileController : ControllerBase
{
    private readonly IAgencyProfileAppFunction _agencyProfileAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public AgencyProfileController(IAgencyProfileAppFunction __agencyProfileAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _agencyProfileAppFunction = __agencyProfileAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<AgencyProfileResponse>>> Get(CancellationToken __cancellationToken)
    {
        var result = await _agencyProfileAppFunction.GetAsync(__cancellationToken);
        return Ok(ApiResponse<AgencyProfileResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<AgencyProfileResponse>>> Update(UpdateAgencyProfileRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _agencyProfileAppFunction.UpdateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<AgencyProfileResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Agency profile updated."));
    }
}
