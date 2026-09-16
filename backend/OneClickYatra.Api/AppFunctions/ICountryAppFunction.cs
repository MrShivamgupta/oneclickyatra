using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface ICountryAppFunction
{
    Task<PaginationResponse<CountryResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<CountryResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CountryResponse> CreateAsync(CountryRequest __request, CancellationToken __cancellationToken);
    Task<CountryResponse> UpdateAsync(Guid __id, CountryRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
