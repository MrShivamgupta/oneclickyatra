using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IPackageAppFunction
{
    Task<PaginationResponse<PackageResponse>> SearchAsync(PackageSearchRequest __request, CancellationToken __cancellationToken);
    Task<PackageDetailResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PackageDetailResponse> GetBySlugAsync(string __slug, CancellationToken __cancellationToken);
    Task<PackageResponse> CreateAsync(PackageRequest __request, CancellationToken __cancellationToken);
    Task<PackageResponse> UpdateAsync(Guid __id, PackageRequest __request, CancellationToken __cancellationToken);
    Task<PackageResponse> UpdateStatusAsync(Guid __id, PackageStatusRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);

    Task<IReadOnlyList<PackageItineraryDayResponse>> ReplaceItineraryAsync(Guid __packageId, IReadOnlyList<PackageItineraryDayRequest> __days, CancellationToken __cancellationToken);
    Task<IReadOnlyList<PackageInclusionResponse>> ReplaceInclusionsAsync(Guid __packageId, IReadOnlyList<PackageInclusionRequest> __inclusions, CancellationToken __cancellationToken);
    Task<IReadOnlyList<PackagePricingTierResponse>> ReplacePricingAsync(Guid __packageId, IReadOnlyList<PackagePricingTierRequest> __tiers, CancellationToken __cancellationToken);
    Task<IReadOnlyList<PackageInventoryResponse>> ReplaceInventoryAsync(Guid __packageId, IReadOnlyList<PackageInventoryRequest> __departures, CancellationToken __cancellationToken);
    Task<IReadOnlyList<PackageMediaResponse>> ReplaceMediaAsync(Guid __packageId, IReadOnlyList<PackageMediaRequest> __media, CancellationToken __cancellationToken);
}
