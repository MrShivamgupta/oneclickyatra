using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface ICityRepository
{
    Task<CityModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CityModel?> GetByCountryAndNameAsync(Guid __countryId, string __name, CancellationToken __cancellationToken);
    Task<PaginationResponse<CityModel>> ListAsync(PaginationRequest __request, Guid? __countryId, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(CityModel __city, CancellationToken __cancellationToken);
    Task UpdateAsync(CityModel __city, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
