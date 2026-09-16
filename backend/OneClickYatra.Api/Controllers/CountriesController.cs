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
[Route("api/v1/countries")]
[Authorize]
public sealed class CountriesController : ControllerBase
{
    private readonly ICountryAppFunction _countryAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public CountriesController(ICountryAppFunction __countryAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _countryAppFunction = __countryAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<CountryResponse>>>> List([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _countryAppFunction.ListAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<CountryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataView)]
    public async Task<ActionResult<ApiResponse<CountryResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _countryAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<CountryResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<CountryResponse>>> Create(CountryRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _countryAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<CountryResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Country created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<CountryResponse>>> Update(Guid id, CountryRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _countryAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<CountryResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Country updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.MasterDataManage)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _countryAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Country deleted."));
    }
}
