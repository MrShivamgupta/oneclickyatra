using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

/// <summary>
/// Staff-facing admin CRUD for the Vendor Management domain (Hotels/Airlines/Transport/DMC/
/// Activity Providers). Sub-resource actions (contacts, rates, payments, performance) are gated
/// by the same four vendor.* permissions as the Vendor entity itself: view for every read, update
/// for every write to an existing vendor's data, create/delete reserved for the vendor row itself.
/// </summary>
[ApiController]
[Route("api/v1/vendors")]
[Authorize]
public sealed class VendorsController : ControllerBase
{
    private readonly IVendorAppFunction _vendorAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public VendorsController(IVendorAppFunction __vendorAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _vendorAppFunction = __vendorAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<VendorResponse>>>> Search([FromQuery] VendorSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<VendorResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<VendorResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.VendorCreate)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Create(VendorRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<VendorResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> Update(Guid id, VendorRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<VendorResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.VendorDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _vendorAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Vendor deleted."));
    }

    /// <summary>Links an existing Vendor row to a User account so that user can sign into the
    /// Vendor Portal. There is no vendor self-registration flow yet — an admin performs this once
    /// a login has been provisioned for the vendor another way (e.g. UserManage).</summary>
    [HttpPost("{id:guid}/link-user")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<VendorResponse>>> LinkUser(Guid id, VendorLinkUserRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.LinkUserAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<VendorResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor linked to user."));
    }

    [HttpGet("{id:guid}/contacts")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<List<VendorContactResponse>>>> GetContacts(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.GetContactsAsync(id, __cancellationToken);
        return Ok(ApiResponse<List<VendorContactResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("{id:guid}/contacts")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<List<VendorContactResponse>>>> ReplaceContacts(Guid id, List<VendorContactRequest> __contacts, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.ReplaceContactsAsync(id, __contacts, __cancellationToken);
        return Ok(ApiResponse<List<VendorContactResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Contacts saved."));
    }

    [HttpGet("{id:guid}/rates")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<List<VendorRateResponse>>>> GetRates(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.GetRatesAsync(id, __cancellationToken);
        return Ok(ApiResponse<List<VendorRateResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("{id:guid}/rates")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<List<VendorRateResponse>>>> ReplaceRates(Guid id, List<VendorRateRequest> __rates, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.ReplaceRatesAsync(id, __rates, __cancellationToken);
        return Ok(ApiResponse<List<VendorRateResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Rates saved."));
    }

    [HttpGet("{id:guid}/payments")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<VendorPaymentResponse>>>> GetPayments(Guid id, [FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.GetPaymentsAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<VendorPaymentResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("{id:guid}/payments")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<VendorPaymentResponse>>> CreatePayment(Guid id, VendorPaymentRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.CreatePaymentAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<VendorPaymentResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor payment recorded."));
    }

    [HttpPut("{id:guid}/payments/{paymentId:guid}/status")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<VendorPaymentResponse>>> UpdatePaymentStatus(Guid id, Guid paymentId, VendorPaymentStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.UpdatePaymentStatusAsync(id, paymentId, __request, __cancellationToken);
        return Ok(ApiResponse<VendorPaymentResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor payment status updated."));
    }

    [HttpPost("{id:guid}/performance")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<VendorPerformanceResponse>>> RecordPerformance(Guid id, VendorPerformanceRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.RecordPerformanceAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<VendorPerformanceResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor performance recorded."));
    }

    [HttpGet("{id:guid}/performance")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<List<VendorPerformanceResponse>>>> GetPerformanceHistory(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.GetPerformanceHistoryAsync(id, __cancellationToken);
        return Ok(ApiResponse<List<VendorPerformanceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}/invoices")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<VendorInvoiceResponse>>>> GetInvoices(Guid id, [FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.GetInvoicesAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<VendorInvoiceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPut("{id:guid}/invoices/{invoiceId:guid}/status")]
    [HasPermission(PermissionConstants.VendorUpdate)]
    public async Task<ActionResult<ApiResponse<VendorInvoiceResponse>>> UpdateInvoiceStatus(Guid id, Guid invoiceId, VendorInvoiceStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _vendorAppFunction.UpdateInvoiceStatusAsync(id, invoiceId, __request, __cancellationToken);
        return Ok(ApiResponse<VendorInvoiceResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Vendor invoice status updated."));
    }

    [HttpGet("invoices/{invoiceId:guid}/download")]
    [HasPermission(PermissionConstants.VendorView)]
    public async Task<IActionResult> DownloadInvoice(Guid invoiceId, CancellationToken __cancellationToken)
    {
        var (content, contentType, fileName) = await _vendorAppFunction.DownloadInvoiceAsync(invoiceId, __cancellationToken);
        return File(content, contentType, fileName);
    }
}
