using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface ISeasonRepository
{
    Task<SeasonModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<SeasonModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken);
    Task<PaginationResponse<SeasonModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(SeasonModel __season, CancellationToken __cancellationToken);
    Task UpdateAsync(SeasonModel __season, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
