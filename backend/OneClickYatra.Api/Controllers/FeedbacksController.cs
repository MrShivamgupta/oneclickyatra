using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

/// <summary>
/// Staff-facing admin read-side for customer feedback. The write-side (customer submission) lives
/// on the Customer Portal (POST /api/v1/portal/bookings/{id}/feedback) and is untouched here.
/// </summary>
[ApiController]
[Route("api/v1/feedbacks")]
[Authorize]
public sealed class FeedbacksController : ControllerBase
{
    private readonly IFeedbackAppFunction _feedbackAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public FeedbacksController(IFeedbackAppFunction __feedbackAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _feedbackAppFunction = __feedbackAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.FeedbackView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<FeedbackListResponse>>>> Search([FromQuery] FeedbackSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _feedbackAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<FeedbackListResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }
}
