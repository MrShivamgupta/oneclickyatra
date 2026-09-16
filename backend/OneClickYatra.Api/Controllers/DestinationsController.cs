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
[Route("api/v1/destinations")]
public sealed class DestinationsController : ControllerBase
{
    private readonly IDestinationAppFunction _destinationAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public DestinationsController(IDestinationAppFunction __destinationAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _destinationAppFunction = __destinationAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Public: used by both the admin CRUD list and the public destination listing page.</summary>
    [HttpGet]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<DestinationResponse>>>> Search([FromQuery] DestinationSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _destinationAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<DestinationResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("search")]
    [EnableRateLimiting("public")]
    public Task<ActionResult<ApiResponse<PaginationResponse<DestinationResponse>>>> SearchAlias([FromQuery] DestinationSearchRequest __request, CancellationToken __cancellationToken) =>
        Search(__request, __cancellationToken);

    [HttpGet("{id:guid}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<DestinationResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _destinationAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<DestinationResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Public: pretty public-site URL lookup, e.g. /destinations/goa-india. Never returns unpublished content.</summary>
    [HttpGet("by-slug/{slug}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<DestinationResponse>>> GetBySlug(string slug, CancellationToken __cancellationToken)
    {
        var result = await _destinationAppFunction.GetBySlugAsync(slug, __cancellationToken);
        return Ok(ApiResponse<DestinationResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [Authorize]
    [HasPermission(PermissionConstants.DestinationCreate)]
    public async Task<ActionResult<ApiResponse<DestinationResponse>>> Create(DestinationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _destinationAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<DestinationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Destination created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.DestinationUpdate)]
    public async Task<ActionResult<ApiResponse<DestinationResponse>>> Update(Guid id, DestinationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _destinationAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<DestinationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Destination updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.DestinationDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _destinationAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Destination deleted."));
    }
}
