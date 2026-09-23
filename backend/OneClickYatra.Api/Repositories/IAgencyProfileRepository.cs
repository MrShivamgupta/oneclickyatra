using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IAgencyProfileRepository
{
    /// <summary>The single non-deleted row, or null if the seed somehow never ran.</summary>
    Task<AgencyProfileModel?> GetAsync(CancellationToken __cancellationToken);
    Task UpdateAsync(AgencyProfileModel __profile, CancellationToken __cancellationToken);
}
