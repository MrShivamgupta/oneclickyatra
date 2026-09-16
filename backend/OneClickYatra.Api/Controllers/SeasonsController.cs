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
[Route("api/v1/seasons")]
[Authorize]
public sealed class SeasonsController : ControllerBase
{
    private readonly ISeasonAppFunction _seasonAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public SeasonsController(ISeasonAppFunction __seasonAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _seasonAppFunction = __seasonAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<SeasonResponse>>>> List([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _seasonAppFunction.ListAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<SeasonResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _seasonAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<SeasonResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> Create(SeasonRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _seasonAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<SeasonResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Season created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<SeasonResponse>>> Update(Guid id, SeasonRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _seasonAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<SeasonResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Season updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _seasonAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Season deleted."));
    }
}
