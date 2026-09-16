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
[Route("api/v1/cities")]
[Authorize]
public sealed class CitiesController : ControllerBase
{
    private readonly ICityAppFunction _cityAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public CitiesController(ICityAppFunction __cityAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _cityAppFunction = __cityAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<CityResponse>>>> List(
        [FromQuery] PaginationRequest __request, [FromQuery] Guid? countryId, CancellationToken __cancellationToken)
    {
        var result = await _cityAppFunction.ListAsync(__request, countryId, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<CityResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<CityResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _cityAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<CityResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<CityResponse>>> Create(CityRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _cityAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<CityResponse>.Ok(result, _trackingIdAccessor.TrackingId, "City created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<CityResponse>>> Update(Guid id, CityRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _cityAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<CityResponse>.Ok(result, _trackingIdAccessor.TrackingId, "City updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _cityAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "City deleted."));
    }
}
