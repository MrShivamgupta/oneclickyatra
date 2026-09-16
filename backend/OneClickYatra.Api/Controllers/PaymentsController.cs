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
[Route("api/v1/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentAppFunction _paymentAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public PaymentsController(IPaymentAppFunction __paymentAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _paymentAppFunction = __paymentAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.PaymentView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<PaymentResponse>>>> Search([FromQuery] PaymentSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<PaymentResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.PaymentView)]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("initiate")]
    [HasPermission(PermissionConstants.PaymentCreate)]
    public async Task<ActionResult<ApiResponse<PaymentInitiateResponse>>> Initiate(PaymentInitiateRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.InitiateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaymentInitiateResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Payment order created."));
    }

    [HttpPost("refund")]
    [HasPermission(PermissionConstants.PaymentRefund)]
    public async Task<ActionResult<ApiResponse<RefundResponse>>> Refund(PaymentRefundRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.InitiateRefundAsync(__request, __cancellationToken);
        return Ok(ApiResponse<RefundResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Refund initiated."));
    }

    /// <summary>Public: called by Razorpay's servers, never by a browser. Trust comes entirely from
    /// the HMAC signature, not from a JWT — there is no user to authenticate here.</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> Webhook(CancellationToken __cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(__cancellationToken);

        var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
        var eventId = Request.Headers["X-Razorpay-Event-Id"].FirstOrDefault();

        await _paymentAppFunction.ProcessWebhookAsync(rawBody, signature, eventId, __cancellationToken);
        return Ok();
    }
}
