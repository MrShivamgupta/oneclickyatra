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
[Route("api/v1/pages")]
public sealed class PagesController : ControllerBase
{
    private readonly IPageAppFunction _pageAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public PagesController(IPageAppFunction __pageAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _pageAppFunction = __pageAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Public: used by the public site to render CMS content pages (About/Contact/Terms/Privacy, etc).</summary>
    [HttpGet("public/{slug}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<PageResponse>>> GetPublishedBySlug(string slug, CancellationToken __cancellationToken)
    {
        var result = await _pageAppFunction.GetPublishedBySlugAsync(slug, __cancellationToken);
        return Ok(ApiResponse<PageResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet]
    [Authorize]
    [HasPermission(PermissionConstants.CmsView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<PageResponse>>>> List([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _pageAppFunction.ListAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<PageResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.CmsView)]
    public async Task<ActionResult<ApiResponse<PageResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _pageAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<PageResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [Authorize]
    [HasPermission(PermissionConstants.CmsManage)]
    public async Task<ActionResult<ApiResponse<PageResponse>>> Create(PageRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _pageAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PageResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Page created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.CmsManage)]
    public async Task<ActionResult<ApiResponse<PageResponse>>> Update(Guid id, PageRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _pageAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<PageResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Page updated."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(PermissionConstants.CmsManage)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _pageAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Page deleted."));
    }
}
