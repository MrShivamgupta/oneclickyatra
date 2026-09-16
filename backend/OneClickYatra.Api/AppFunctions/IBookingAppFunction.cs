using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IBookingAppFunction
{
    Task<PaginationResponse<BookingResponse>> SearchAsync(BookingSearchRequest __request, CancellationToken __cancellationToken);
    Task<List<BookingResponse>> GetUpcomingDeparturesAsync(int __withinDays, int __top, CancellationToken __cancellationToken);
    Task<BookingDetailResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<BookingResponse> CreateAsync(BookingRequest __request, CancellationToken __cancellationToken);
    Task<BookingResponse> UpdateAsync(Guid __id, BookingRequest __request, CancellationToken __cancellationToken);
    Task<BookingResponse> UpdateStatusAsync(Guid __id, BookingStatusRequest __request, CancellationToken __cancellationToken);
    Task<BookingResponse> CancelAsync(Guid __id, BookingCancelRequest __request, CancellationToken __cancellationToken);
    Task<BookingResponse> InitiateRefundAsync(Guid __id, BookingRefundRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);

    Task<List<BookingPassengerResponse>> ReplacePassengersAsync(Guid __id, List<BookingPassengerRequest> __passengers, CancellationToken __cancellationToken);
    Task<List<BookingAddOnResponse>> ReplaceAddOnsAsync(Guid __id, List<BookingAddOnRequest> __addOns, CancellationToken __cancellationToken);
    Task<List<BookingStatusHistoryResponse>> GetStatusHistoryAsync(Guid __id, CancellationToken __cancellationToken);

    Task<BookingResponse> ConvertFromQuotationAsync(Guid __quotationId, CancellationToken __cancellationToken);
}
