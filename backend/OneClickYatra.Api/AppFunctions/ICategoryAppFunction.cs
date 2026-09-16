using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface ICategoryAppFunction
{
    Task<PaginationResponse<CategoryResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<CategoryResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CategoryResponse> CreateAsync(CategoryRequest __request, CancellationToken __cancellationToken);
    Task<CategoryResponse> UpdateAsync(Guid __id, CategoryRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
