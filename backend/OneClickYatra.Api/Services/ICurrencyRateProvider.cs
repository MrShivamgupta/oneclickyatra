using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Services;

public interface ICurrencyRateProvider
{
    Task<CurrencyRatesResponse> GetRatesAsync(CancellationToken __cancellationToken);
}
