using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface ISeasonAppFunction
{
    Task<PaginationResponse<SeasonResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<SeasonResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<SeasonResponse> CreateAsync(SeasonRequest __request, CancellationToken __cancellationToken);
    Task<SeasonResponse> UpdateAsync(Guid __id, SeasonRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
