using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface ICountryRepository
{
    Task<CountryModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CountryModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken);
    Task<PaginationResponse<CountryModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(CountryModel __country, CancellationToken __cancellationToken);
    Task UpdateAsync(CountryModel __country, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
