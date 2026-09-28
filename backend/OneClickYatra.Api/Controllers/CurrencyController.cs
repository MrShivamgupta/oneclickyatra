using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/currency")]
public sealed class CurrencyController : ControllerBase
{
    private readonly ICurrencyRateProvider _currencyRateProvider;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public CurrencyController(ICurrencyRateProvider __currencyRateProvider, ITrackingIdAccessor __trackingIdAccessor)
    {
        _currencyRateProvider = __currencyRateProvider;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Public: live INR exchange rates for the public site's currency switcher (display only, not used for any financial calculation).</summary>
    [HttpGet("rates")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<CurrencyRatesResponse>>> GetRates(CancellationToken __cancellationToken)
    {
        var result = await _currencyRateProvider.GetRatesAsync(__cancellationToken);
        return Ok(ApiResponse<CurrencyRatesResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }
}
