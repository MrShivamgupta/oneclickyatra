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

    [HttpGet]
    [HasPermission(PermissionConstants.QuotationView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<QuotationResponse>>>> Search([FromQuery] QuotationSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<QuotationResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.QuotationView)]
    public async Task<ActionResult<ApiResponse<QuotationDetailResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<QuotationDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.QuotationCreate)]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> Create(QuotationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Quotation created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> Update(Guid id, QuotationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Quotation updated."));
    }

    [HttpPut("{id:guid}/options")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    public async Task<ActionResult<ApiResponse<List<QuotationOptionResponse>>>> ReplaceOptions(Guid id, List<QuotationOptionRequest> __options, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.ReplaceOptionsAsync(id, __options, __cancellationToken);
        return Ok(ApiResponse<List<QuotationOptionResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Options saved."));
    }

    [HttpPost("{id:guid}/send")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> Send(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.SendAsync(id, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Quotation sent."));
    }

    [HttpPost("{id:guid}/regenerate-link")]
    [HasPermission(PermissionConstants.QuotationUpdate)]
    public async Task<ActionResult<ApiResponse<QuotationResponse>>> RegenerateLink(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.RegenerateLinkAsync(id, __cancellationToken);
        return Ok(ApiResponse<QuotationResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Share link regenerated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.QuotationDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _quotationAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Quotation deleted."));
    }

    [HttpGet("{id:guid}/pdf")]
    [HasPermission(PermissionConstants.QuotationView)]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken __cancellationToken)
    {
        var bytes = await _quotationAppFunction.GeneratePdfAsync(id, __cancellationToken);
        return File(bytes, "application/pdf", $"Quotation-{id}.pdf");
    }

    /// <summary>Public: the customer-facing quotation view opened from a shared link. No authentication required.</summary>
    [HttpGet("public/{token}")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<QuotationPublicResponse>>> GetPublic(string token, CancellationToken __cancellationToken)
    {
        var result = await _quotationAppFunction.GetPublicAsync(token, __cancellationToken);
        return Ok(ApiResponse<QuotationPublicResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("public/{token}/approve")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<object?>>> ApprovePublic(string token, QuotationPublicApproveRequest __request, CancellationToken __cancellationToken)
    {
        await _quotationAppFunction.ApprovePublicAsync(token, __request, HttpContext.Connection.RemoteIpAddress?.ToString(), __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Quotation approved. Thank you!"));
    }

    [HttpPost("public/{token}/reject")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<ApiResponse<object?>>> RejectPublic(string token, QuotationPublicRejectRequest __request, CancellationToken __cancellationToken)
    {
        await _quotationAppFunction.RejectPublicAsync(token, __request, HttpContext.Connection.RemoteIpAddress?.ToString(), __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Quotation declined."));
    }

    [HttpGet("public/{token}/pdf")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> DownloadPublicPdf(string token, CancellationToken __cancellationToken)
    {
        var bytes = await _quotationAppFunction.GeneratePublicPdfAsync(token, __cancellationToken);
        return File(bytes, "application/pdf", "Quotation.pdf");
    }
}
