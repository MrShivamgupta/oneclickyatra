using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IVendorPortalAppFunction
{
    Task<VendorResponse> GetMyProfileAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<VendorResponse> UpdateMyProfileAsync(Guid __userId, VendorRequest __request, CancellationToken __cancellationToken);
    Task<List<VendorRateResponse>> GetMyRatesAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<PaginationResponse<VendorPaymentResponse>> GetMyPaymentsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);

    /// <summary>Best-effort "booking requests" feed — see VendorBookingRequestModel for the
    /// scope note on how this approximates a real vendor-assignment feed.</summary>
    Task<PaginationResponse<VendorBookingRequestResponse>> GetMyBookingRequestsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);

    Task<VendorInvoiceResponse> SubmitInvoiceAsync(Guid __userId, Stream __content, string __fileName, string __contentType, decimal __amount, string? __notes, Guid? __bookingId, CancellationToken __cancellationToken);
    Task<PaginationResponse<VendorInvoiceResponse>> GetMyInvoicesAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);
}
