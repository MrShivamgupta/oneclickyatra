using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IPageAppFunction
{
    Task<PageResponse> GetPublishedBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<PaginationResponse<PageResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<PageResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PageResponse> CreateAsync(PageRequest __request, CancellationToken __cancellationToken);
    Task<PageResponse> UpdateAsync(Guid __id, PageRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
