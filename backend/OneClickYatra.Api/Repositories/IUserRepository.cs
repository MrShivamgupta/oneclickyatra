using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IUserRepository
{
    Task<UserModel?> GetByEmailAsync(string __email, CancellationToken __cancellationToken);
    Task<UserModel?> GetByIdAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(UserModel __user, CancellationToken __cancellationToken);
    Task<IReadOnlyList<UserModel>> ListStaffAsync(CancellationToken __cancellationToken);
    Task<PaginationResponse<UserModel>> SearchAsync(UserSearchRequest __request, CancellationToken __cancellationToken);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid __userId, CancellationToken __cancellationToken);
    Task AssignRoleAsync(Guid __userId, string __roleName, CancellationToken __cancellationToken);
    Task RemoveRoleAsync(Guid __userId, string __roleName, CancellationToken __cancellationToken);
    Task SetActiveStatusAsync(Guid __userId, bool __isActive, CancellationToken __cancellationToken);
    Task RecordFailedLoginAsync(Guid __userId, int __maxAttempts, TimeSpan __lockoutDuration, CancellationToken __cancellationToken);
    Task ResetFailedLoginAsync(Guid __userId, CancellationToken __cancellationToken);
    Task UpdatePasswordHashAsync(Guid __userId, string __passwordHash, CancellationToken __cancellationToken);
}
