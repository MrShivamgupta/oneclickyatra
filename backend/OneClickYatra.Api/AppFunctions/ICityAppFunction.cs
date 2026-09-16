using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface ICityAppFunction
{
    Task<PaginationResponse<CityResponse>> ListAsync(PaginationRequest __request, Guid? __countryId, CancellationToken __cancellationToken);
    Task<CityResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CityResponse> CreateAsync(CityRequest __request, CancellationToken __cancellationToken);
    Task<CityResponse> UpdateAsync(Guid __id, CityRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
