using System.Text.Json;
using StackExchange.Redis;

namespace OneClickYatra.Api.Services;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer __connectionMultiplexer, ILogger<RedisCacheService> __logger)
    {
        _connectionMultiplexer = __connectionMultiplexer;
        _logger = __logger;
    }

    public async Task<T?> GetAsync<T>(string __key, CancellationToken __cancellationToken) where T : class
    {
        if (!IsAvailable())
        {
            return null;
        }

        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            var value = await database.StringGetAsync(__key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : null;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Cache GET failed for key {CacheKey}; treating as a miss.", __key);
            return null;
        }
    }

    public async Task SetAsync<T>(string __key, T __value, TimeSpan __ttl, CancellationToken __cancellationToken) where T : class
    {
        if (!IsAvailable())
        {
            return;
        }

        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            await database.StringSetAsync(__key, JsonSerializer.Serialize(__value), __ttl);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Cache SET failed for key {CacheKey}; continuing without caching it.", __key);
        }
    }

    public async Task RemoveAsync(string __key, CancellationToken __cancellationToken)
    {
        if (!IsAvailable())
        {
            return;
        }

        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            await database.KeyDeleteAsync(__key);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Cache DELETE failed for key {CacheKey}.", __key);
        }
    }

    public async Task RemoveByPrefixAsync(string __prefix, CancellationToken __cancellationToken)
    {
        if (!IsAvailable())
        {
            return;
        }

        try
        {
            foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
            {
                var server = _connectionMultiplexer.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: $"{__prefix}*"))
                {
                    await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(key);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Cache prefix invalidation failed for prefix {CachePrefix}.", __prefix);
        }
    }

    /// <summary>
    /// Cheap, in-memory connection-state check (no network round trip). Skipping the call
    /// entirely when Redis is known to be down turns "cache unavailable" from a multi-second
    /// per-command timeout into a no-op, which matters a lot on write-heavy endpoints that touch
    /// the cache 2-3 times per request.
    /// </summary>
    private bool IsAvailable()
    {
        if (_connectionMultiplexer.IsConnected)
        {
            return true;
        }

        _logger.LogDebug("Redis is not connected; skipping cache operation.");
        return false;
    }
}
