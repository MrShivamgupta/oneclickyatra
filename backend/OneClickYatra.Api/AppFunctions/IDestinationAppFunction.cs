using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IDestinationAppFunction
{
    Task<PaginationResponse<DestinationResponse>> SearchAsync(DestinationSearchRequest __request, CancellationToken __cancellationToken);
    Task<DestinationResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<DestinationResponse> GetBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<DestinationResponse> CreateAsync(DestinationRequest __request, CancellationToken __cancellationToken);
    Task<DestinationResponse> UpdateAsync(Guid __id, DestinationRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
