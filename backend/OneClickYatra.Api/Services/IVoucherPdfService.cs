using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Services;

public interface IVoucherPdfService
{
    Task<byte[]> GenerateAsync(BookingDetailResponse __booking, CancellationToken __cancellationToken);
}
