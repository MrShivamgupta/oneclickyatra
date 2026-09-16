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
[Route("api/v1/followups")]
[Authorize]
public sealed class FollowUpsController : ControllerBase
{
    private readonly IFollowUpAppFunction _followUpAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public FollowUpsController(IFollowUpAppFunction __followUpAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _followUpAppFunction = __followUpAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Used by the "Today's Follow-Ups" dashboard widget.</summary>
    [HttpGet("today")]
    [HasPermission(PermissionConstants.FollowUpView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<FollowUpResponse>>>> Today([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _followUpAppFunction.ListTodayAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<FollowUpResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(PermissionConstants.FollowUpUpdate)]
    public async Task<ActionResult<ApiResponse<FollowUpResponse>>> UpdateStatus(Guid id, FollowUpStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _followUpAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<FollowUpResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Follow-up updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.FollowUpDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _followUpAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Follow-up deleted."));
    }
}
