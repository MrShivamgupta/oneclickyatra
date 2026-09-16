using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IRefreshTokenRepository
{
    Task CreateAsync(RefreshTokenModel __token, CancellationToken __cancellationToken);
    Task<RefreshTokenModel?> GetByTokenHashAsync(string __tokenHash, CancellationToken __cancellationToken);
    Task RevokeAsync(Guid __id, string? __replacedByTokenHash, CancellationToken __cancellationToken);
    Task RevokeAllForUserAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<int> DeleteExpiredAsync(CancellationToken __cancellationToken);
}
