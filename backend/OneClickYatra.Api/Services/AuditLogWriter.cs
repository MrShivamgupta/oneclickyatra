using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.Services;

public sealed class AuditLogWriter : IAuditLogWriter
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public AuditLogWriter(
        IAuditLogRepository __auditLogRepository,
        IHttpContextAccessor __httpContextAccessor,
        ITrackingIdAccessor __trackingIdAccessor)
    {
        _auditLogRepository = __auditLogRepository;
        _httpContextAccessor = __httpContextAccessor;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    public Task LogAsync(
        Guid? __userId,
        string __action,
        string __entityName,
        string? __entityId,
        string? __oldValue,
        string? __newValue,
        CancellationToken __cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var entry = new AuditLogModel
        {
            Id = Guid.NewGuid(),
            UserId = __userId,
            Action = __action,
            EntityName = __entityName,
            EntityId = __entityId,
            OldValue = __oldValue,
            NewValue = __newValue,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            TrackingId = _trackingIdAccessor.TrackingId
        };

        return _auditLogRepository.CreateAsync(entry, __cancellationToken);
    }
}
