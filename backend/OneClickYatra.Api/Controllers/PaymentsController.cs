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

    /// <summary>Searches recorded payments with filtering (booking, status) and pagination, per <see cref="PaymentSearchRequest"/>.</summary>
    [HttpGet]
    [HasPermission(PermissionConstants.PaymentView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginationResponse<PaymentResponse>>))]
    public async Task<ActionResult<ApiResponse<PaginationResponse<PaymentResponse>>>> Search([FromQuery] PaymentSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<PaymentResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Gets a single payment record by id.</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.PaymentView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaymentResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<PaymentResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<PaymentResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Creates a Razorpay order for a booking's outstanding balance and returns the ids the
    /// frontend needs to open Razorpay Checkout. Only valid for a booking currently in PendingPayment
    /// status with a positive outstanding balance. Fails with 503 if no Razorpay API keys are configured
    /// in this environment, or 502 if Razorpay itself rejects the order-creation call.</summary>
    [HttpPost("initiate")]
    [HasPermission(PermissionConstants.PaymentCreate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaymentInitiateResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<PaymentInitiateResponse>>> Initiate(PaymentInitiateRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.InitiateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaymentInitiateResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Payment order created."));
    }

    /// <summary>Initiates a gateway refund for a booking's captured payment. Only valid for a booking
    /// currently in RefundPending status with a captured payment that has a recorded gateway payment id.
    /// Card/UPI refunds are typically instant and auto-complete the booking to Refunded; netbanking
    /// refunds are left Processing for an admin to confirm once Razorpay settles them. Fails with 503 if
    /// Razorpay isn't configured, or 502 if Razorpay rejects the refund call.</summary>
    [HttpPost("refund")]
    [HasPermission(PermissionConstants.PaymentRefund)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<RefundResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status502BadGateway, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<RefundResponse>>> Refund(PaymentRefundRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _paymentAppFunction.InitiateRefundAsync(__request, __cancellationToken);
        return Ok(ApiResponse<RefundResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Refund initiated."));
    }

    /// <summary>Public: called by Razorpay's servers, never by a browser. Trust comes entirely from
    /// the HMAC signature, not from a JWT — there is no user to authenticate here. Idempotent per
    /// gateway event id: a duplicate delivery of the same event is acknowledged and skipped rather than
    /// reprocessed. Unknown event types/order ids are also acknowledged without action rather than erroring.</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
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
