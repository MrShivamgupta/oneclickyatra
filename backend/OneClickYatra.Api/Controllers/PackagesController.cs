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
[Route("api/v1/packages")]
public sealed class PackagesController : ControllerBase
{
    private readonly IPackageAppFunction _packageAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public PackagesController(IPackageAppFunction __packageAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _packageAppFunction = __packageAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Public: used by both the admin package list and the public package search page.</summary>
    [HttpGet]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<PackageResponse>>>> Search([FromQuery] PackageSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<PackageResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("search")]
    [EnableRateLimiting("public")]
    public Task<ActionResult<ApiResponse<PaginationResponse<PackageResponse>>>> SearchAlias([FromQuery] PackageSearchRequest __request, CancellationToken __cancellationToken) =>
        Search(__request, __cancellationToken);

    /// <summary>Public: full package detail including itinerary/inclusions/pricing/inventory/media.</summary>
    [HttpGet("{id:guid}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<PackageDetailResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<PackageDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Public: pretty public-site URL lookup, e.g. /packages/goa-family-getaway. Never returns non-Published packages.</summary>
    [HttpGet("by-slug/{slug}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<PackageDetailResponse>>> GetBySlug(string slug, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.GetBySlugAsync(slug, __cancellationToken);
        return Ok(ApiResponse<PackageDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [Authorize]
    [HasPermission(PermissionConstants.PackageCreate)]
    public async Task<ActionResult<ApiResponse<PackageResponse>>> Create(PackageRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PackageResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Package created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<PackageResponse>>> Update(Guid id, PackageRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<PackageResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Package updated."));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<PackageResponse>>> UpdateStatus(Guid id, PackageStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<PackageResponse>.Ok(result, _trackingIdAccessor.TrackingId, $"Package marked as {result.Status}."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _packageAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Package deleted."));
    }

    [HttpPut("{id:guid}/itinerary")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PackageItineraryDayResponse>>>> ReplaceItinerary(
        Guid id, List<PackageItineraryDayRequest> __days, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.ReplaceItineraryAsync(id, __days, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PackageItineraryDayResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Itinerary saved."));
    }

    [HttpPut("{id:guid}/inclusions")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PackageInclusionResponse>>>> ReplaceInclusions(
        Guid id, List<PackageInclusionRequest> __inclusions, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.ReplaceInclusionsAsync(id, __inclusions, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PackageInclusionResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Inclusions saved."));
    }

    [HttpPut("{id:guid}/pricing")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PackagePricingTierResponse>>>> ReplacePricing(
        Guid id, List<PackagePricingTierRequest> __tiers, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.ReplacePricingAsync(id, __tiers, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PackagePricingTierResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Pricing saved."));
    }

    [HttpPut("{id:guid}/inventory")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PackageInventoryResponse>>>> ReplaceInventory(
        Guid id, List<PackageInventoryRequest> __departures, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.ReplaceInventoryAsync(id, __departures, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PackageInventoryResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Inventory saved."));
    }

    [HttpPut("{id:guid}/media")]
    [Authorize]
    [HasPermission(PermissionConstants.PackageUpdate)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PackageMediaResponse>>>> ReplaceMedia(
        Guid id, List<PackageMediaRequest> __media, CancellationToken __cancellationToken)
    {
        var result = await _packageAppFunction.ReplaceMediaAsync(id, __media, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PackageMediaResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Media saved."));
    }
}
