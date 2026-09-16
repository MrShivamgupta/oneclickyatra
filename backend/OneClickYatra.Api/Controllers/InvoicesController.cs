using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
[HasPermission(PermissionConstants.PaymentView)]
public sealed class InvoicesController : ControllerBase
{
    private readonly IInvoiceAppFunction _invoiceAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public InvoicesController(IInvoiceAppFunction __invoiceAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _invoiceAppFunction = __invoiceAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResponse<InvoiceResponse>>>> Search([FromQuery] InvoiceSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _invoiceAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<InvoiceResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _invoiceAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<InvoiceResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken __cancellationToken)
    {
        var bytes = await _invoiceAppFunction.GeneratePdfAsync(id, __cancellationToken);
        return File(bytes, "application/pdf", $"Invoice-{id}.pdf");
    }
}
