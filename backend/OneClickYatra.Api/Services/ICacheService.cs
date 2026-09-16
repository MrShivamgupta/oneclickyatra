namespace OneClickYatra.Api.Services;

/// <summary>
/// Redis-backed cache-aside helper. SQL Server remains the source of truth; every method
/// degrades gracefully (logs a warning, acts as a cache miss) if Redis is unreachable, so a
/// down/unconfigured Redis never breaks a request.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string __key, CancellationToken __cancellationToken) where T : class;
    Task SetAsync<T>(string __key, T __value, TimeSpan __ttl, CancellationToken __cancellationToken) where T : class;
    Task RemoveAsync(string __key, CancellationToken __cancellationToken);
    Task RemoveByPrefixAsync(string __prefix, CancellationToken __cancellationToken);
}
