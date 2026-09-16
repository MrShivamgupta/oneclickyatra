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

    [HttpGet]
    [HasPermission(PermissionConstants.BookingView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<BookingResponse>>>> Search([FromQuery] BookingSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<BookingResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.BookingView)]
    public async Task<ActionResult<ApiResponse<BookingDetailResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<BookingDetailResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}/history")]
    [HasPermission(PermissionConstants.BookingView)]
    public async Task<ActionResult<ApiResponse<List<BookingStatusHistoryResponse>>>> GetHistory(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.GetStatusHistoryAsync(id, __cancellationToken);
        return Ok(ApiResponse<List<BookingStatusHistoryResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.BookingCreate)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Create(BookingRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking created."));
    }

    [HttpPost("from-quotation/{quotationId:guid}")]
    [HasPermission(PermissionConstants.BookingCreate)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> ConvertFromQuotation(Guid quotationId, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.ConvertFromQuotationAsync(quotationId, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking created from quotation."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Update(Guid id, BookingRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking updated."));
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> UpdateStatus(Guid id, BookingStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking status updated."));
    }

    [HttpPost("{id:guid}/cancel")]
    [HasPermission(PermissionConstants.BookingCancel)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Cancel(Guid id, BookingCancelRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.CancelAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Booking cancelled."));
    }

    [HttpPost("{id:guid}/refund")]
    [HasPermission(PermissionConstants.BookingRefund)]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Refund(Guid id, BookingRefundRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.InitiateRefundAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Refund initiated."));
    }

    [HttpPut("{id:guid}/passengers")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    public async Task<ActionResult<ApiResponse<List<BookingPassengerResponse>>>> ReplacePassengers(Guid id, List<BookingPassengerRequest> __passengers, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.ReplacePassengersAsync(id, __passengers, __cancellationToken);
        return Ok(ApiResponse<List<BookingPassengerResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Passengers saved."));
    }

    [HttpPut("{id:guid}/addons")]
    [HasPermission(PermissionConstants.BookingUpdate)]
    public async Task<ActionResult<ApiResponse<List<BookingAddOnResponse>>>> ReplaceAddOns(Guid id, List<BookingAddOnRequest> __addOns, CancellationToken __cancellationToken)
    {
        var result = await _bookingAppFunction.ReplaceAddOnsAsync(id, __addOns, __cancellationToken);
        return Ok(ApiResponse<List<BookingAddOnResponse>>.Ok(result, _trackingIdAccessor.TrackingId, "Add-ons saved."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.BookingDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _bookingAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Booking deleted."));
    }
}
