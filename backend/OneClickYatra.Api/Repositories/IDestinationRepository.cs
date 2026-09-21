using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IDestinationRepository
{
    Task<DestinationModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<IReadOnlyList<Guid>> GetExistingIdsAsync(IReadOnlyList<Guid> __ids, CancellationToken __cancellationToken);
    Task<DestinationModel?> GetBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<PaginationResponse<DestinationModel>> SearchAsync(DestinationSearchRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(DestinationModel __destination, CancellationToken __cancellationToken);
    Task UpdateAsync(DestinationModel __destination, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
