using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>Recurring Hangfire job: purges expired/revoked refresh tokens so the table doesn't grow unbounded.</summary>
public sealed class TokenCleanupJob
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(IRefreshTokenRepository __refreshTokenRepository, ILogger<TokenCleanupJob> __logger)
    {
        _refreshTokenRepository = __refreshTokenRepository;
        _logger = __logger;
    }

    public async Task RunAsync(CancellationToken __cancellationToken = default)
    {
        var deletedCount = await _refreshTokenRepository.DeleteExpiredAsync(__cancellationToken);
        _logger.LogInformation("TokenCleanupJob removed {DeletedCount} expired/revoked refresh tokens.", deletedCount);
    }
}
