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
[Route("api/v1/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingAppFunction _bookingAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public BookingsController(IBookingAppFunction __bookingAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _bookingAppFunction = __bookingAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Searches bookings with filtering (status, customer, destination, date range) and pagination, per <see cref="BookingSearchRequest"/>.</summary>
    [HttpGet]
    [HasPermission(PermissionConstants.BookingView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginationResponse<BookingResponse>>))]
    public async Task<ActionResult<ApiResponse<PaginationResponse<BookingResponse>>>> Search([FromQuery] BookingSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<BookingResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Gets a single booking's full detail — the booking record plus its passengers and add-ons.</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.BookingView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingDetailResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<BookingDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Gets the booking's append-only status-change history (old/new status, who changed it, when, and why) in chronological order.</summary>
    [HttpGet("{id:guid}/history")]
    [HasPermission(PermissionConstants.BookingView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<BookingStatusHistoryResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<List<BookingStatusHistoryResponse>>>> GetHistory(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.GetStatusHistoryAsync(id, __cancellationToken);
        return Ok(ApiResponse<List<BookingStatusHistoryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Creates a new booking, always starting in Draft status. Verifies the customer exists and,
    /// if supplied, that the optional lead/package/destination references resolve, before saving.</summary>
    [HttpPost]
    [HasPermission(PermissionConstants.BookingCreate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Create(BookingRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking created."));
    }

    /// <summary>Converts an Approved quotation's selected option into a new booking (starting in Quoted status),
    /// auto-creating a Customer from the quotation's lead when one doesn't already exist, and marks the source
    /// quotation Converted. Fails if the quotation isn't Approved or has no selected option.</summary>
    [HttpPost("from-quotation/{quotationId:guid}")]
    [HasPermission(PermissionConstants.BookingCreate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> ConvertFromQuotation(Guid quotationId, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.ConvertFromQuotationAsync(quotationId, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking created from quotation."));
    }

    /// <summary>Updates a booking's package/destination/dates/traveler counts/amount/notes. Only allowed while
    /// the booking is in an editable status (Draft through InProgress) — a Completed/Cancelled/Refunded booking
    /// can no longer be edited this way.</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Update(Guid id, BookingRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking updated."));
    }

    /// <summary>Moves a booking to a new status through the booking state machine (see
    /// <c>BookingAppFunction.AllowedTransitions</c>). Rejects any transition not in that fixed table —
    /// e.g. jumping straight from Draft to Confirmed.</summary>
    [HttpPut("{id:guid}/status")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> UpdateStatus(Guid id, BookingStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking status updated."));
    }

    /// <summary>Cancels a booking (a valid transition from any pre-InProgress status) and records the supplied
    /// reason as the booking's cancellation reason.</summary>
    [HttpPost("{id:guid}/cancel")]
    [HasPermission(PermissionConstants.BookingCancel)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Cancel(Guid id, BookingCancelRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.CancelAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking cancelled."));
    }

    /// <summary>Moves a booking into RefundPending — the first step of the refund workflow. The actual
    /// gateway-side refund call is a separate step (see <c>PaymentsController.Refund</c>); only valid from
    /// Confirmed or Cancelled.</summary>
    [HttpPost("{id:guid}/refund")]
    [HasPermission(PermissionConstants.BookingRefund)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Refund(Guid id, BookingRefundRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.InitiateRefundAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Refund initiated."));
    }

    /// <summary>Replaces the booking's full passenger list in one call (delete-then-insert, not a merge —
    /// omitting a passenger removes them). Only allowed while the booking is in an editable status.</summary>
    [HttpPut("{id:guid}/passengers")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<BookingPassengerResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<List<BookingPassengerResponse>>>> ReplacePassengers(Guid id, List<BookingPassengerRequest> __passengers, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.ReplacePassengersAsync(id, __passengers, __cancellationToken);
        return Ok(ApiResponse<List<BookingPassengerResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Passengers saved."));
    }

    /// <summary>Replaces the booking's full add-on list in one call (delete-then-insert, not a merge —
    /// omitting an add-on removes it). Only allowed while the booking is in an editable status.</summary>
    [HttpPut("{id:guid}/addons")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<BookingAddOnResponse>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<List<BookingAddOnResponse>>>> ReplaceAddOns(Guid id, List<BookingAddOnRequest> __addOns, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.ReplaceAddOnsAsync(id, __addOns, __cancellationToken);
        return Ok(ApiResponse<List<BookingAddOnResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Add-ons saved."));
    }

    /// <summary>Permanently deletes a booking. Only a Draft booking can be deleted — anything further along
    /// must be Cancelled instead so the status-history audit trail is preserved.</summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.BookingDelete)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _bookingAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Booking deleted."));
    }
}
