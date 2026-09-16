using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/enquiries")]
public sealed class EnquiriesController : ControllerBase
{
    private readonly IEnquiryAppFunction _enquiryAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public EnquiriesController(IEnquiryAppFunction __enquiryAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _enquiryAppFunction = __enquiryAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Public: the enquiry capture form on the public site. No authentication required.</summary>
    [HttpPost]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<EnquiryResponse>>> Create(EnquiryRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _enquiryAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<EnquiryResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Enquiry submitted."));
    }

    [HttpGet]
    [Authorize]
    [HasPermission(PermissionConstants.EnquiryView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<EnquiryResponse>>>> Search([FromQuery] EnquirySearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _enquiryAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<EnquiryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.EnquiryView)]
    public async Task<ActionResult<ApiResponse<EnquiryResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _enquiryAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<EnquiryResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize]
    [HasPermission(PermissionConstants.EnquiryUpdate)]
    public async Task<ActionResult<ApiResponse<EnquiryResponse>>> UpdateStatus(Guid id, EnquiryStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _enquiryAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<EnquiryResponse>.Ok(result, _trackingIdAccessor.TrackingId, $"Enquiry marked as {result.Status}."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.EnquiryDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _enquiryAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Enquiry deleted."));
    }
}
