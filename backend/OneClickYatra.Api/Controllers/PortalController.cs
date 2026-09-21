using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Controllers;

/// <summary>
/// The customer-facing self-service portal. Gated by [Authorize] only, deliberately not
/// [HasPermission] — Customer accounts hold no RBAC permissions at all (see Seed001), so every
/// method here authorizes by ownership ("is this my own data"), not by permission claim.
/// </summary>
[ApiController]
[Route("api/v1/portal")]
[Authorize]
public sealed class PortalController : ControllerBase
{
    private readonly ICustomerPortalAppFunction _portalAppFunction;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public PortalController(ICustomerPortalAppFunction __portalAppFunction, ICurrentUserAccessor __currentUserAccessor, ITrackingIdAccessor __trackingIdAccessor)
    {
        _portalAppFunction = __portalAppFunction;
        _currentUserAccessor = __currentUserAccessor;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    private Guid CurrentUserId => _currentUserAccessor.UserId ?? throw new InvalidCredentialsException();

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetProfile(CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetMyProfileAsync(CurrentUserId, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> UpdateProfile(CustomerRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.UpdateMyProfileAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Profile updated."));
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<BookingResponse>>>> GetBookings([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetMyBookingsAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<BookingResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("bookings/{id:guid}")]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> GetBookingById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetMyBookingByIdAsync(CurrentUserId, id, __cancellationToken);
        return Ok(ApiResponse<BookingDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("bookings/{id:guid}/feedback")]
    public async Task<ActionResult<ApiResponse<FeedbackResponse>>> SubmitFeedback(Guid id, FeedbackRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.SubmitFeedbackAsync(CurrentUserId, id, __request, __cancellationToken);
        return Ok(ApiResponse<FeedbackResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Thank you for your feedback!"));
    }

    [HttpGet("payments")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<PaymentResponse>>>> GetPayments([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetMyPaymentsAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<PaymentResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<InvoiceResponse>>>> GetInvoices([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetMyInvoicesAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<InvoiceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("invoices/{id:guid}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(Guid id, CancellationToken __cancellationToken)
    {
        var bytes = await _portalAppFunction.GetMyInvoicePdfAsync(CurrentUserId, id, __cancellationToken);
        return File(bytes, "application/pdf", $"Invoice-{id}.pdf");
    }

    [HttpGet("quotations")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<QuotationResponse>>>> GetQuotations([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetMyQuotationsAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<QuotationResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("bookings/{id:guid}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<CustomerDocumentResponse>>> UploadDocument(Guid id, IFormFile file, CancellationToken __cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await _portalAppFunction.UploadDocumentAsync(CurrentUserId, id, stream, file.FileName, file.ContentType, __cancellationToken);
        return Ok(ApiResponse<CustomerDocumentResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Document uploaded."));
    }

    [HttpGet("bookings/{id:guid}/documents")]
    public async Task<ActionResult<ApiResponse<List<CustomerDocumentResponse>>>> GetDocuments(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _portalAppFunction.GetDocumentsAsync(CurrentUserId, id, __cancellationToken);
        return Ok(ApiResponse<List<CustomerDocumentResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("documents/{documentId:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid documentId, CancellationToken __cancellationToken)
    {
        var (content, contentType, fileName) = await _portalAppFunction.DownloadDocumentAsync(CurrentUserId, documentId, __cancellationToken);
        return File(content, contentType, fileName);
    }

    [HttpGet("bookings/{id:guid}/voucher")]
    public async Task<IActionResult> DownloadVoucher(Guid id, CancellationToken __cancellationToken)
    {
        var bytes = await _portalAppFunction.GetVoucherPdfAsync(CurrentUserId, id, __cancellationToken);
        return File(bytes, "application/pdf", $"Voucher-{id}.pdf");
    }
}
