using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IFollowUpAppFunction
{
    Task<IReadOnlyList<FollowUpResponse>> ListByLeadAsync(Guid __leadId, CancellationToken __cancellationToken);
    Task<PaginationResponse<FollowUpResponse>> ListTodayAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<FollowUpResponse> CreateAsync(Guid __leadId, FollowUpRequest __request, CancellationToken __cancellationToken);
    Task<FollowUpResponse> UpdateStatusAsync(Guid __id, FollowUpStatusRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
