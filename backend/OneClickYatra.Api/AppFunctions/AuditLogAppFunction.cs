using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// Thin pass-through over <see cref="IAuditLogRepository"/>'s SearchAsync — a pure read with no
/// business-rule branching, mirroring ReportAppFunction's minimal style. Audit log writes go
/// through IAuditLogWriter from wherever an action needs to be recorded (see BookingAppFunction);
/// this is the read side that closes the loop for the admin audit-log viewer.
/// </summary>
public sealed class AuditLogAppFunction : IAuditLogAppFunction
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogAppFunction(IAuditLogRepository __auditLogRepository)
    {
        _auditLogRepository = __auditLogRepository;
    }

    public async Task<PaginationResponse<AuditLogResponse>> SearchAsync(AuditLogSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _auditLogRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<AuditLogResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    private static AuditLogResponse ToResponse(AuditLogModel __model) => new()
    {
        Id = __model.Id,
        ActorUserId = __model.UserId,
        ActorName = CombineActorName(__model.ActorName, __model.ActorEmail),
        Action = __model.Action,
        EntityName = __model.EntityName,
        EntityId = __model.EntityId,
        OldValue = __model.OldValue,
        NewValue = __model.NewValue,
        IpAddress = __model.IpAddress,
        CreatedAt = __model.CreatedAt
    };

    private static string? CombineActorName(string? __actorName, string? __actorEmail)
    {
        if (__actorName is null)
        {
            return null;
        }

        return __actorEmail is null ? __actorName : $"{__actorName} ({__actorEmail})";
    }
}
