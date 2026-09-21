using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/quotations")]
[Authorize]
public sealed class QuotationsController : ControllerBase
{
    private readonly IQuotationAppFunction _quotationAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public QuotationsController(IQuotationAppFunction __quotationAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _quotationAppFunction = __quotationAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Searches quotations with filtering (status, lead, customer) and pagination, per <see cref="QuotationSearchRequest"/>.</summary>
    [HttpGet]
    [HasPermission(PermissionConstants.QuotationView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginationResponse<QuotationResponse>>))]
    public async Task<ActionResult<ApiResponse<PaginationResponse<QuotationResponse>>>> Search([FromQuery] QuotationSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<QuotationResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Gets a single quotation's full detail — the quotation plus all of its priced options and their line items.</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.QuotationView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuotationDetailResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<QuotationDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Creates a new quotation for a lead, starting in Draft status, with a fresh hashed public
    /// share-link token (the raw token is only ever returned once, in this call's response).</summary>
    [HttpPost]
    [HasPermission(PermissionConstants.QuotationCreate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuotationResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> Create(QuotationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Quotation created."));
    }

    /// <summary>Updates a quotation's title/valid-until/notes. Only allowed while the quotation is still
    /// Draft, Sent or Expired — once a customer has Approved or Rejected it via the public link, it can no
    /// longer be edited.</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuotationResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> Update(Guid id, QuotationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Quotation updated."));
    }

    /// <summary>Replaces the quotation's full set of priced options (and their line items) in one call
    /// (delete-then-insert, not a merge). Validates every referenced Destination/Package exists first.
    /// Only allowed while the quotation is still editable (Draft, Sent or Expired).</summary>
    [HttpPut("{id:guid}/options")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<QuotationOptionResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<List<QuotationOptionResponse>>>> ReplaceOptions(Guid id, List<QuotationOptionRequest> __options, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.ReplaceOptionsAsync(id, __options, __cancellationToken);
        return Ok(ApiResponse<List<QuotationOptionResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Options saved."));
    }

    /// <summary>Sends a Draft quotation to the customer, moving it to Sent. Requires at least one option
    /// already saved on it — an empty quotation cannot be sent.</summary>
    [HttpPost("{id:guid}/send")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuotationResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> Send(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.SendAsync(id, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Quotation sent."));
    }

    /// <summary>Issues a brand-new public share-link token for this quotation, invalidating the previous
    /// one (e.g. after an accidental leak). The new raw token is only ever returned once, in this call's response.</summary>
    [HttpPost("{id:guid}/regenerate-link")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuotationResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> RegenerateLink(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.RegenerateLinkAsync(id, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Share link regenerated."));
    }

    /// <summary>Permanently deletes a quotation. Only Draft, Rejected or Expired quotations can be deleted —
    /// a Sent or Approved/Converted quotation must be left in place for the audit trail.</summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.QuotationDelete)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _quotationAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Quotation deleted."));
    }

    /// <summary>Generates and downloads the staff-facing PDF for a quotation (same layout as the public PDF).</summary>
    [HttpGet("{id:guid}/pdf")]
    [HasPermission(PermissionConstants.QuotationView)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken __cancellationToken)
    {
        var bytes = await _quotationAppFunction.GeneratePdfAsync(id, __cancellationToken);
        return File(bytes, "application/pdf", $"Quotation-{id}.pdf");
    }

    /// <summary>Public: the customer-facing quotation view opened from a shared link. No authentication required.
    /// A Draft quotation (not yet sent) behaves as not-found — the token only becomes resolvable once it's Sent.
    /// A Sent quotation past its ValidUntil date is auto-flipped to Expired on this read.</summary>
    [HttpGet("public/{token}")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<QuotationPublicResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<QuotationPublicResponse>>> GetPublic(string token, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.GetPublicAsync(token, __cancellationToken);
        return Ok(ApiResponse<QuotationPublicResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Public: the customer's digital approval of one option on a Sent quotation, via the shared
    /// link. No authentication — the customer's name/comments and the calling IP are recorded on the approval
    /// record instead. Fails if the quotation is no longer Sent (already decided or expired) or the selected
    /// option doesn't belong to this quotation.</summary>
    [HttpPost("public/{token}/approve")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> ApprovePublic(string token, QuotationPublicApproveRequest __request, CancellationToken __cancellationToken)
    {
        await _quotationAppFunction.ApprovePublicAsync(token, __request, HttpContext.Connection.RemoteIpAddress?.ToString(), __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Quotation approved. Thank you!"));
    }

    /// <summary>Public: the customer's digital decline of a Sent quotation, via the shared link. No
    /// authentication required. Fails if the quotation is no longer Sent (already decided or expired).</summary>
    [HttpPost("public/{token}/reject")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> RejectPublic(string token, QuotationPublicRejectRequest __request, CancellationToken __cancellationToken)
    {
        await _quotationAppFunction.RejectPublicAsync(token, __request, HttpContext.Connection.RemoteIpAddress?.ToString(), __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Quotation declined."));
    }

    /// <summary>Public: downloads the same quotation PDF as the staff-facing endpoint, resolved by share-link
    /// token instead of a JWT-authenticated id. Same Draft/expired-token semantics as GetPublic.</summary>
    [HttpGet("public/{token}/pdf")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<IActionResult> DownloadPublicPdf(string token, CancellationToken __cancellationToken)
    {
        var bytes = await _quotationAppFunction.GeneratePublicPdfAsync(token, __cancellationToken);
        return File(bytes, "application/pdf", "Quotation.pdf");
    }
}
