using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IRefundRepository
{
    Task<RefundModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<IReadOnlyList<RefundModel>> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task CreateAsync(RefundModel __refund, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, string? __gatewayRefundId, Guid? __updatedBy, CancellationToken __cancellationToken);
}
