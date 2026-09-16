using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IPageRepository
{
    Task<PageModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PageModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<PaginationResponse<PageModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(PageModel __page, CancellationToken __cancellationToken);
    Task UpdateAsync(PageModel __page, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
