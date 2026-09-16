using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IPaymentRepository
{
    Task<PaymentModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PaymentModel?> GetByGatewayOrderIdAsync(string __gatewayOrderId, CancellationToken __cancellationToken);
    Task<IReadOnlyList<PaymentModel>> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task<PaginationResponse<PaymentModel>> SearchAsync(PaymentSearchRequest __request, CancellationToken __cancellationToken);
    Task CreateAsync(PaymentModel __payment, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, string? __gatewayPaymentId, CancellationToken __cancellationToken);

    /// <summary>Inserts an event ledger row. Returns false (no-op) instead of throwing when
    /// GatewayEventId already exists — the unique index is the source of truth for idempotency.</summary>
    Task<bool> AddTransactionAsync(PaymentTransactionModel __transaction, CancellationToken __cancellationToken);
}
