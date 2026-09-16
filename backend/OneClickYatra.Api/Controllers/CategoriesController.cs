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
[Route("api/v1/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryAppFunction _categoryAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public CategoriesController(ICategoryAppFunction __categoryAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _categoryAppFunction = __categoryAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<CategoryResponse>>>> List([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _categoryAppFunction.ListAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<CategoryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _categoryAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<CategoryResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Create(CategoryRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _categoryAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<CategoryResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Category created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> Update(Guid id, CategoryRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _categoryAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<CategoryResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Category updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _categoryAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Category deleted."));
    }
}
