using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

/// <summary>
/// Read-only admin viewer over AuditLogs. Entries are written exclusively through IAuditLogWriter
/// from wherever an action needs to be recorded (e.g. BookingAppFunction) — this controller only
/// surfaces them back for review, gated by the existing audit.view permission.
/// </summary>
[ApiController]
[Route("api/v1/audit-logs")]
[Authorize]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogAppFunction _auditLogAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public AuditLogsController(IAuditLogAppFunction __auditLogAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _auditLogAppFunction = __auditLogAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.AuditView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<AuditLogResponse>>>> Search([FromQuery] AuditLogSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _auditLogAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<AuditLogResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }
}
