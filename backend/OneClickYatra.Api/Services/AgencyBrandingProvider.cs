using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.Services;

public sealed class AgencyBrandingProvider : IAgencyBrandingProvider
{
    private static readonly AgencyProfileModel FallbackProfile = new() { Name = "One Click Yatra", Currency = "INR" };

    private readonly IAgencyProfileRepository _agencyProfileRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgencyBrandingProvider> _logger;

    public AgencyBrandingProvider(HttpClient __httpClient, IAgencyProfileRepository __agencyProfileRepository, ILogger<AgencyBrandingProvider> __logger)
    {
        _httpClient = __httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _agencyProfileRepository = __agencyProfileRepository;
        _logger = __logger;
    }

    public async Task<AgencyProfileModel> GetProfileAsync(CancellationToken __cancellationToken)
    {
        try
        {
            var profile = await _agencyProfileRepository.GetAsync(__cancellationToken);
            return profile ?? FallbackProfile;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not load the agency profile; generating this document with the default branding instead.");
            return FallbackProfile;
        }
    }

    public async Task<byte[]?> TryFetchLogoBytesAsync(string? __logoUrl, CancellationToken __cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(__logoUrl))
        {
            return null;
        }

        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(__logoUrl, __cancellationToken);
            return LooksLikeAnImage(bytes) ? bytes : null;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not fetch the agency logo from {LogoUrl}; generating this document without it.", __logoUrl);
            return null;
        }
    }

    // A cheap magic-byte sanity check (PNG/JPEG signatures) rather than a full image decode -- good
    // enough to catch the realistic failure mode (a broken URL serving an HTML error page) without
    // taking on a new image-decoding dependency just for this.
    private static bool LooksLikeAnImage(byte[] __bytes)
    {
        if (__bytes.Length < 8) return false;

        var isPng = __bytes[0] == 0x89 && __bytes[1] == 0x50 && __bytes[2] == 0x4E && __bytes[3] == 0x47;
        var isJpeg = __bytes[0] == 0xFF && __bytes[1] == 0xD8 && __bytes[2] == 0xFF;
        return isPng || isJpeg;
    }
}
