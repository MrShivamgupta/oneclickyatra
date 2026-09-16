using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

public sealed class UserAppFunction : IUserAppFunction
{
    private readonly IUserRepository _userRepository;

    public UserAppFunction(IUserRepository __userRepository)
    {
        _userRepository = __userRepository;
    }

    public async Task<IReadOnlyList<UserSummaryResponse>> ListStaffAsync(CancellationToken __cancellationToken)
    {
        var users = await _userRepository.ListStaffAsync(__cancellationToken);
        return users.Select(u => new UserSummaryResponse { Id = u.Id, FullName = u.FullName, Email = u.Email }).ToList();
    }
}
