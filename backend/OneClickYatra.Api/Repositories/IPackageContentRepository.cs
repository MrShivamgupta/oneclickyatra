using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

/// <summary>
/// Manages the child collections of a Package (itinerary/inclusions/pricing/inventory/media).
/// Each collection is edited as a whole from the package builder screen, so writes use a
/// replace-all pattern (delete existing rows for the package, insert the new set) inside a
/// transaction, rather than granular per-row CRUD endpoints.
/// </summary>
public interface IPackageContentRepository
{
    Task<IReadOnlyList<PackageItineraryDayModel>> GetItineraryAsync(Guid __packageId, CancellationToken __cancellationToken);
    Task ReplaceItineraryAsync(Guid __packageId, IReadOnlyList<PackageItineraryDayModel> __days, CancellationToken __cancellationToken);

    Task<IReadOnlyList<PackageInclusionModel>> GetInclusionsAsync(Guid __packageId, CancellationToken __cancellationToken);
    Task ReplaceInclusionsAsync(Guid __packageId, IReadOnlyList<PackageInclusionModel> __inclusions, CancellationToken __cancellationToken);

    Task<IReadOnlyList<PackagePricingTierModel>> GetPricingAsync(Guid __packageId, CancellationToken __cancellationToken);
    Task ReplacePricingAsync(Guid __packageId, IReadOnlyList<PackagePricingTierModel> __tiers, CancellationToken __cancellationToken);

    Task<IReadOnlyList<PackageInventoryModel>> GetInventoryAsync(Guid __packageId, CancellationToken __cancellationToken);
    Task ReplaceInventoryAsync(Guid __packageId, IReadOnlyList<PackageInventoryModel> __departures, CancellationToken __cancellationToken);

    Task<IReadOnlyList<PackageMediaModel>> GetMediaAsync(Guid __packageId, CancellationToken __cancellationToken);
    Task ReplaceMediaAsync(Guid __packageId, IReadOnlyList<PackageMediaModel> __media, CancellationToken __cancellationToken);
}
