using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IUserAppFunction
{
    Task<IReadOnlyList<UserSummaryResponse>> ListStaffAsync(CancellationToken __cancellationToken);
    Task<PaginationResponse<UserResponse>> SearchAsync(UserSearchRequest __request, CancellationToken __cancellationToken);
    Task<UserResponse> CreateAsync(CreateStaffUserRequest __request, CancellationToken __cancellationToken);
    Task<UserResponse> UpdateRoleAsync(Guid __id, UpdateUserRoleRequest __request, CancellationToken __cancellationToken);
    Task<UserResponse> UpdateStatusAsync(Guid __id, UpdateUserStatusRequest __request, CancellationToken __cancellationToken);
}
