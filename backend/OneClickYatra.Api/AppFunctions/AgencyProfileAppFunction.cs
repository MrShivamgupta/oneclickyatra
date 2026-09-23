using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>Admin read/update over the single AgencyProfile row (see Seed021 -- there is always
/// exactly one). If it's ever missing (a mis-seeded environment), that's a real setup problem worth
/// surfacing here rather than silently defaulting -- unlike IAgencyBrandingProvider, which PDF
/// generation uses and which DOES fall back safely, since a customer-facing PDF should never fail
/// just because this admin screen's backing row is missing.</summary>
public sealed class AgencyProfileAppFunction : IAgencyProfileAppFunction
{
    private readonly IAgencyProfileRepository _agencyProfileRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AgencyProfileAppFunction(IAgencyProfileRepository __agencyProfileRepository, ICurrentUserAccessor __currentUserAccessor)
    {
        _agencyProfileRepository = __agencyProfileRepository;
        _currentUserAccessor = __currentUserAccessor;
    }

    public async Task<AgencyProfileResponse> GetAsync(CancellationToken __cancellationToken)
    {
        var profile = await _agencyProfileRepository.GetAsync(__cancellationToken)
            ?? throw new EntityNotFoundException("AgencyProfile", "singleton");
        return ToResponse(profile);
    }

    public async Task<AgencyProfileResponse> UpdateAsync(UpdateAgencyProfileRequest __request, CancellationToken __cancellationToken)
    {
        var profile = await _agencyProfileRepository.GetAsync(__cancellationToken)
            ?? throw new EntityNotFoundException("AgencyProfile", "singleton");

        profile.Name = __request.Name;
        profile.LogoUrl = __request.LogoUrl;
        profile.Address = __request.Address;
        profile.GstNumber = __request.GstNumber;
        profile.Currency = __request.Currency;
        profile.SupportEmail = __request.SupportEmail;
        profile.SupportPhone = __request.SupportPhone;
        profile.UpdatedBy = _currentUserAccessor.UserId;

        await _agencyProfileRepository.UpdateAsync(profile, __cancellationToken);
        return await GetAsync(__cancellationToken);
    }

    private static AgencyProfileResponse ToResponse(Models.AgencyProfileModel profile) => new()
    {
        Name = profile.Name,
        LogoUrl = profile.LogoUrl,
        Address = profile.Address,
        GstNumber = profile.GstNumber,
        Currency = profile.Currency,
        SupportEmail = profile.SupportEmail,
        SupportPhone = profile.SupportPhone,
        UpdatedAt = profile.UpdatedAt
    };
}
