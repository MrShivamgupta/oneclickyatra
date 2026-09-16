using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface ICategoryRepository
{
    Task<CategoryModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CategoryModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<PaginationResponse<CategoryModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(CategoryModel __category, CancellationToken __cancellationToken);
    Task UpdateAsync(CategoryModel __category, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
