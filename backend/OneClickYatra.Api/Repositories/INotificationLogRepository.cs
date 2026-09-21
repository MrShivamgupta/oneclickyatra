using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface INotificationLogRepository
{
    Task<NotificationLogModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PaginationResponse<NotificationLogModel>> SearchAsync(NotificationLogSearchRequest __request, CancellationToken __cancellationToken);

    /// <summary>Inserts a log row. Returns false (no-op) instead of throwing when GatewayMessageId
    /// already exists — the unique index is the source of truth for inbound-webhook idempotency.</summary>
    Task<bool> CreateAsync(NotificationLogModel __log, CancellationToken __cancellationToken);
}
