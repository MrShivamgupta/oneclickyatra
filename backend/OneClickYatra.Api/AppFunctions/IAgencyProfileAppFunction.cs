using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IAgencyProfileAppFunction
{
    Task<AgencyProfileResponse> GetAsync(CancellationToken __cancellationToken);
    Task<AgencyProfileResponse> UpdateAsync(UpdateAgencyProfileRequest __request, CancellationToken __cancellationToken);
}
