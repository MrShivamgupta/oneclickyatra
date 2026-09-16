namespace OneClickYatra.Api.Globals;

/// <summary>Standard envelope every API endpoint returns.</summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string TrackingId { get; init; } = string.Empty;

    public static ApiResponse<T> Ok(T? __data, string __trackingId, string __message = "Operation completed successfully") => new()
    {
        Success = true,
        Data = __data,
        Message = __message,
        TrackingId = __trackingId
    };

    public static ApiResponse<T> Fail(string __trackingId, string __message) => new()
    {
        Success = false,
        Data = default,
        Message = __message,
        TrackingId = __trackingId
    };
}

/// <summary>Non-generic error envelope for endpoints that only need to signal failure.</summary>
public sealed class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public object? Data { get; init; } = null;
    public string Message { get; init; } = string.Empty;
    public string TrackingId { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    public static ApiErrorResponse Create(string __trackingId, string __message, IReadOnlyDictionary<string, string[]>? __errors = null) => new()
    {
        Message = __message,
        TrackingId = __trackingId,
        Errors = __errors
    };
}
