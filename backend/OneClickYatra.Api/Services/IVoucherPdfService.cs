using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Services;

public interface IVoucherPdfService
{
    byte[] Generate(BookingDetailResponse __booking);
}
