using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IBookingRepository
{
    Task<BookingModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PaginationResponse<BookingModel>> SearchAsync(BookingSearchRequest __request, CancellationToken __cancellationToken);
    Task<IReadOnlyList<BookingModel>> GetUpcomingDeparturesAsync(int __withinDays, int __top, CancellationToken __cancellationToken);
    Task CreateAsync(BookingModel __booking, CancellationToken __cancellationToken);
    Task UpdateAsync(BookingModel __booking, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task AddAmountPaidAsync(Guid __id, decimal __amount, CancellationToken __cancellationToken);
    Task SetCancellationReasonAsync(Guid __id, string? __reason, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);

    Task<IReadOnlyList<BookingPassengerModel>> GetPassengersAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task ReplacePassengersAsync(Guid __bookingId, IReadOnlyList<BookingPassengerModel> __passengers, CancellationToken __cancellationToken);

    Task<IReadOnlyList<BookingAddOnModel>> GetAddOnsAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task ReplaceAddOnsAsync(Guid __bookingId, IReadOnlyList<BookingAddOnModel> __addOns, CancellationToken __cancellationToken);

    Task AddStatusHistoryAsync(BookingStatusHistoryModel __history, CancellationToken __cancellationToken);
    Task<IReadOnlyList<BookingStatusHistoryModel>> GetStatusHistoryAsync(Guid __bookingId, CancellationToken __cancellationToken);
}
