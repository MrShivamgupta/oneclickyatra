using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IPackageRepository
{
    Task<PackageModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PackageModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<PaginationResponse<PackageModel>> SearchAsync(PackageSearchRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(PackageModel __package, CancellationToken __cancellationToken);
    Task UpdateAsync(PackageModel __package, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
