using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IVendorAppFunction
{
    Task<PaginationResponse<VendorResponse>> SearchAsync(VendorSearchRequest __request, CancellationToken __cancellationToken);
    Task<VendorResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<VendorResponse> CreateAsync(VendorRequest __request, CancellationToken __cancellationToken);
    Task<VendorResponse> UpdateAsync(Guid __id, VendorRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
    Task<VendorResponse> LinkUserAsync(Guid __id, VendorLinkUserRequest __request, CancellationToken __cancellationToken);

    Task<List<VendorContactResponse>> GetContactsAsync(Guid __id, CancellationToken __cancellationToken);
    Task<List<VendorContactResponse>> ReplaceContactsAsync(Guid __id, List<VendorContactRequest> __contacts, CancellationToken __cancellationToken);

    Task<List<VendorRateResponse>> GetRatesAsync(Guid __id, CancellationToken __cancellationToken);
    Task<List<VendorRateResponse>> ReplaceRatesAsync(Guid __id, List<VendorRateRequest> __rates, CancellationToken __cancellationToken);

    Task<PaginationResponse<VendorPaymentResponse>> GetPaymentsAsync(Guid __id, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<VendorPaymentResponse> CreatePaymentAsync(Guid __id, VendorPaymentRequest __request, CancellationToken __cancellationToken);
    Task<VendorPaymentResponse> UpdatePaymentStatusAsync(Guid __id, Guid __paymentId, VendorPaymentStatusRequest __request, CancellationToken __cancellationToken);

    Task<VendorPerformanceResponse> RecordPerformanceAsync(Guid __id, VendorPerformanceRequest __request, CancellationToken __cancellationToken);
    Task<List<VendorPerformanceResponse>> GetPerformanceHistoryAsync(Guid __id, CancellationToken __cancellationToken);

    Task<PaginationResponse<VendorInvoiceResponse>> GetInvoicesAsync(Guid __id, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<VendorInvoiceResponse> UpdateInvoiceStatusAsync(Guid __id, Guid __invoiceId, VendorInvoiceStatusRequest __request, CancellationToken __cancellationToken);
    Task<(Stream Content, string ContentType, string FileName)> DownloadInvoiceAsync(Guid __invoiceId, CancellationToken __cancellationToken);
}
