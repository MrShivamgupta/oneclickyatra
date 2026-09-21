using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// Backs the customer-facing portal. Every method resolves the caller's own Customer record from
/// their logged-in UserId — there is no permission system involved (Customer accounts hold no
/// permissions at all; see RBAC seed), only "is this the caller's own data", checked explicitly
/// in each method rather than relying on RBAC.
/// </summary>
public interface ICustomerPortalAppFunction
{
    Task<CustomerResponse> GetMyProfileAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<CustomerResponse> UpdateMyProfileAsync(Guid __userId, CustomerRequest __request, CancellationToken __cancellationToken);
    Task<PaginationResponse<BookingResponse>> GetMyBookingsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<BookingDetailResponse> GetMyBookingByIdAsync(Guid __userId, Guid __bookingId, CancellationToken __cancellationToken);
    Task<PaginationResponse<PaymentResponse>> GetMyPaymentsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<PaginationResponse<InvoiceResponse>> GetMyInvoicesAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<byte[]> GetMyInvoicePdfAsync(Guid __userId, Guid __invoiceId, CancellationToken __cancellationToken);
    Task<PaginationResponse<QuotationResponse>> GetMyQuotationsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<FeedbackResponse> SubmitFeedbackAsync(Guid __userId, Guid __bookingId, FeedbackRequest __request, CancellationToken __cancellationToken);
    Task<CustomerDocumentResponse> UploadDocumentAsync(Guid __userId, Guid __bookingId, Stream __content, string __fileName, string __contentType, CancellationToken __cancellationToken);
    Task<List<CustomerDocumentResponse>> GetDocumentsAsync(Guid __userId, Guid __bookingId, CancellationToken __cancellationToken);
    Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(Guid __userId, Guid __documentId, CancellationToken __cancellationToken);
    Task<byte[]> GetVoucherPdfAsync(Guid __userId, Guid __bookingId, CancellationToken __cancellationToken);
}
