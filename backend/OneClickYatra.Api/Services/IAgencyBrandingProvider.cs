using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Services;

/// <summary>What every PDF-generating service (invoice/quotation/voucher) needs to brand its
/// header/footer with the real, admin-editable agency name and logo instead of a hardcoded string.
/// Deliberately tolerant of failure -- a slow/unreachable/invalid logo URL must never break a
/// customer-facing PDF generation; callers get null and fall back to a text-only header.</summary>
public interface IAgencyBrandingProvider
{
    /// <summary>Never throws -- falls back to a sensible default (Name="One Click Yatra") if the
    /// AgencyProfile row is somehow missing, since PDF generation must never fail over this.</summary>
    Task<AgencyProfileModel> GetProfileAsync(CancellationToken __cancellationToken);

    /// <summary>Fetches the logo image bytes for embedding via QuestPDF's Image(byte[]). Returns null
    /// (never throws) if the URL is empty, unreachable, slow, or doesn't look like a real image.</summary>
    Task<byte[]?> TryFetchLogoBytesAsync(string? __logoUrl, CancellationToken __cancellationToken);
}
