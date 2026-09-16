using System.Data.Common;
using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Data.SqlClient;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Services.Payments;
using ValidationException = FluentValidation.ValidationException;

namespace OneClickYatra.Api.Middleware;

/// <summary>
/// Centralized exception handler. Converts every exception into the standard ApiErrorResponse
/// envelope and never leaks SQL errors, stack traces, connection strings or other internals.
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate __next, ILogger<ExceptionMiddleware> __logger)
    {
        _next = __next;
        _logger = __logger;
    }

    public async Task InvokeAsync(HttpContext __context)
    {
        try
        {
            await _next(__context);
        }
        catch (Exception exception)
        {
            await HandleAsync(__context, exception);
        }
    }

    private async Task HandleAsync(HttpContext __context, Exception __exception)
    {
        var trackingId = __context.Items[TrackingIdMiddleware.ItemsKey] as string ?? Guid.NewGuid().ToString("N");

        var (statusCode, message, errors) = Map(__exception);

        _logger.LogError(__exception, "Unhandled exception. TrackingId={TrackingId} StatusCode={StatusCode}", trackingId, statusCode);

        __context.Response.ContentType = "application/json";
        __context.Response.StatusCode = statusCode;

        var response = ApiErrorResponse.Create(trackingId, message, errors);
        await __context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static (int StatusCode, string Message, IReadOnlyDictionary<string, string[]>? Errors) Map(Exception __exception) => __exception switch
    {
        ValidationAppException validationAppException => ((int)HttpStatusCode.UnprocessableEntity, "Validation failed.", validationAppException.Errors),
        ValidationException fluentValidationException => ((int)HttpStatusCode.UnprocessableEntity, "Validation failed.", ToErrorDictionary(fluentValidationException)),
        EntityNotFoundException notFoundException => ((int)HttpStatusCode.NotFound, notFoundException.Message, null),
        InvalidCredentialsException invalidCredentialsException => ((int)HttpStatusCode.Unauthorized, invalidCredentialsException.Message, null),
        AccountLockedException accountLockedException => ((int)HttpStatusCode.Locked, accountLockedException.Message, null),
        InvalidWebhookSignatureException invalidWebhookSignatureException => ((int)HttpStatusCode.BadRequest, invalidWebhookSignatureException.Message, null),
        PaymentGatewayNotConfiguredException notConfiguredException => ((int)HttpStatusCode.ServiceUnavailable, notConfiguredException.Message, null),
        PaymentGatewayException => ((int)HttpStatusCode.BadGateway, "The payment gateway is currently unavailable. Please try again shortly.", null),
        UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "You are not authorized to perform this action.", null),
        BusinessException businessException => ((int)HttpStatusCode.Conflict, businessException.Message, null),
        SqlException => ((int)HttpStatusCode.InternalServerError, "A database error occurred. Please try again later.", null),
        DbException => ((int)HttpStatusCode.InternalServerError, "A database error occurred. Please try again later.", null),
        _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.", null)
    };

    private static IReadOnlyDictionary<string, string[]> ToErrorDictionary(ValidationException __exception) =>
        __exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
}
