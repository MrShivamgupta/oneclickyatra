using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IAuditLogAppFunction
{
    Task<PaginationResponse<AuditLogResponse>> SearchAsync(AuditLogSearchRequest __request, CancellationToken __cancellationToken);
}
