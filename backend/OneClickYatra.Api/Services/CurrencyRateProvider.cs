using System.Text.Json;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Services;

/// <summary>
/// Live INR exchange rates for the public site's currency switcher, backed by open.er-api.com
/// (free, no API key, updates once daily) -- a customer-facing display convenience only, never
/// used for anything financial (bookings/payments/invoices always stay in the agency's own
/// configured Currency). Fails safe at every step: a fixed, clearly-approximate fallback rate set
/// is returned if the external call and the cache both come up empty, so a slow or unreachable
/// third-party API can never break a public page that shows it.
/// </summary>
public sealed class CurrencyRateProvider : ICurrencyRateProvider
{
    private const string CacheKey = "currency:rates:inr";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(12);

    // Only the currencies actually offered in the public switcher -- the upstream API returns
    // ~160, most of which this app has no use for.
    private static readonly string[] OfferedCurrencies = ["USD", "EUR", "GBP", "AUD", "CAD", "SGD", "AED", "JPY"];

    // Deliberately approximate and clearly a last resort (not "live") -- only used if both the
    // cache and a fresh fetch fail, so the switcher still shows something rather than breaking.
    private static readonly CurrencyRatesResponse FallbackRates = new()
    {
        Base = "INR",
        AsOf = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        Rates = new Dictionary<string, decimal>
        {
            ["USD"] = 0.012m,
            ["EUR"] = 0.011m,
            ["GBP"] = 0.0095m,
            ["AUD"] = 0.018m,
            ["CAD"] = 0.016m,
            ["SGD"] = 0.016m,
            ["AED"] = 0.044m,
            ["JPY"] = 1.75m
        }
    };

    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CurrencyRateProvider> _logger;

    public CurrencyRateProvider(HttpClient __httpClient, ICacheService __cacheService, ILogger<CurrencyRateProvider> __logger)
    {
        _httpClient = __httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _cacheService = __cacheService;
        _logger = __logger;
    }

    public async Task<CurrencyRatesResponse> GetRatesAsync(CancellationToken __cancellationToken)
    {
        var cached = await _cacheService.GetAsync<CurrencyRatesResponse>(CacheKey, __cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        try
        {
            var raw = await _httpClient.GetStringAsync("https://open.er-api.com/v6/latest/INR", __cancellationToken);
            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;

            if (!root.TryGetProperty("result", out var resultProperty) || resultProperty.GetString() != "success")
            {
                _logger.LogWarning("open.er-api.com returned a non-success result; falling back to the static rate set.");
                return FallbackRates;
            }

            var ratesElement = root.GetProperty("rates");
            var rates = new Dictionary<string, decimal>();
            foreach (var code in OfferedCurrencies)
            {
                if (ratesElement.TryGetProperty(code, out var rateValue) && rateValue.TryGetDecimal(out var rate))
                {
                    rates[code] = rate;
                }
            }

            if (rates.Count == 0)
            {
                return FallbackRates;
            }

            var response = new CurrencyRatesResponse { Base = "INR", AsOf = DateTime.UtcNow, Rates = rates };
            await _cacheService.SetAsync(CacheKey, response, CacheTtl, __cancellationToken);
            return response;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not fetch live currency rates; falling back to the static rate set.");
            return FallbackRates;
        }
    }
}
