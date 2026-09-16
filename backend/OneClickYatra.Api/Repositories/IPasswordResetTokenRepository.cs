using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IPasswordResetTokenRepository
{
    Task CreateAsync(PasswordResetTokenModel __token, CancellationToken __cancellationToken);
    Task<PasswordResetTokenModel?> GetByTokenHashAsync(string __tokenHash, CancellationToken __cancellationToken);
    Task MarkUsedAsync(Guid __id, CancellationToken __cancellationToken);
}
