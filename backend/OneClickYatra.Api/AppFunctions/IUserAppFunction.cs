using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IUserAppFunction
{
    Task<IReadOnlyList<UserSummaryResponse>> ListStaffAsync(CancellationToken __cancellationToken);
}
