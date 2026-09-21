using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Controllers;

/// <summary>
/// The vendor-facing self-service portal. Gated by [Authorize] only, deliberately not
/// [HasPermission] — Vendor accounts hold no RBAC permissions at all (see Seed011), so every
/// method here authorizes by ownership ("is this my own data"), not by permission claim.
/// </summary>
[ApiController]
[Route("api/v1/vendor-portal")]
[Authorize]
public sealed class VendorPortalController : ControllerBase
{
    private readonly IVendorPortalAppFunction _vendorPortalAppFunction;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public VendorPortalController(IVendorPortalAppFunction __vendorPortalAppFunction, ICurrentUserAccessor __currentUserAccessor, ITrackingIdAccessor __trackingIdAccessor)
    {
        _vendorPortalAppFunction = __vendorPortalAppFunction;
        _currentUserAccessor = __currentUserAccessor;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    private Guid CurrentUserId => _currentUserAccessor.UserId ?? throw new InvalidCredentialsException();

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> GetProfile(CancellationToken __cancellationToken)
    {
        var result = await _vendorPortalAppFunction.GetMyProfileAsync(CurrentUserId, __cancellationToken);
        return Ok(ApiResponse<VendorResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> UpdateProfile(VendorRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorPortalAppFunction.UpdateMyProfileAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<VendorResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Profile updated."));
    }

    [HttpGet("rates")]
    public async Task<ActionResult<ApiResponse<List<VendorRateResponse>>>> GetRates(CancellationToken __cancellationToken)
    {
        var result = await _vendorPortalAppFunction.GetMyRatesAsync(CurrentUserId, __cancellationToken);
        return Ok(ApiResponse<List<VendorRateResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("payments")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<VendorPaymentResponse>>>> GetPayments([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorPortalAppFunction.GetMyPaymentsAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<VendorPaymentResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Best-effort: see VendorBookingRequestModel for the scope note on how this
    /// approximates a real "requests assigned to you" feed via a DestinationId join.</summary>
    [HttpGet("booking-requests")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<VendorBookingRequestResponse>>>> GetBookingRequests([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorPortalAppFunction.GetMyBookingRequestsAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<VendorBookingRequestResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("invoices")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<VendorInvoiceResponse>>> SubmitInvoice(IFormFile file, [FromForm] decimal amount, [FromForm] string? notes, [FromForm] Guid? bookingId, CancellationToken __cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await _vendorPortalAppFunction.SubmitInvoiceAsync(CurrentUserId, stream, file.FileName, file.ContentType, amount, notes, bookingId, __cancellationToken);
        return Ok(ApiResponse<VendorInvoiceResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Invoice submitted."));
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<VendorInvoiceResponse>>>> GetInvoices([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorPortalAppFunction.GetMyInvoicesAsync(CurrentUserId, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<VendorInvoiceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }
}
