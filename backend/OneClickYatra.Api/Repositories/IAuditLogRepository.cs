using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLogModel __entry, CancellationToken __cancellationToken);

    Task<PaginationResponse<AuditLogModel>> SearchAsync(AuditLogSearchRequest __request, CancellationToken __cancellationToken);
}
