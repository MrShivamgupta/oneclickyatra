namespace OneClickYatra.Api.Services;

public interface IAuditLogWriter
{
    Task LogAsync(
        Guid? __userId,
        string __action,
        string __entityName,
        string? __entityId,
        string? __oldValue,
        string? __newValue,
        CancellationToken __cancellationToken);
}
