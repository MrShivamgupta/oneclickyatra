using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLogModel __entry, CancellationToken __cancellationToken);
}
