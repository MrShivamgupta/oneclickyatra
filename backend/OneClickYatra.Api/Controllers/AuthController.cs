using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthAppFunction _authAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public AuthController(IAuthAppFunction __authAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _authAppFunction = __authAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    /// <summary>Authenticates by email/password and issues a JWT access token plus a refresh token.
    /// Locks the account (see <c>AccountLockedException</c>, 423) after too many consecutive failed
    /// attempts, per <c>SecurityOptions.MaxFailedLoginAttempts</c>. Rate-limited under the "auth" policy.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<AuthenticatedUserResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserResponse>>> Login(LoginRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _authAppFunction.LoginAsync(__request, GetClientIp(), __cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Login successful."));
    }

    /// <summary>Self-registers a new account. Every self-registered user is both a "Customer"-role
    /// User and gets a linked Customer CRM record created up front, so the customer portal (bookings,
    /// payments, invoices, quotations) always has one to resolve. Fails if the email is already registered.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<AuthenticatedUserResponse>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserResponse>>> Register(RegisterRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _authAppFunction.RegisterAsync(__request, GetClientIp(), __cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Registration successful."));
    }

    /// <summary>Exchanges a still-valid refresh token for a new access/refresh token pair, rotating the
    /// refresh token (the old one is revoked). Reusing an already-revoked or expired refresh token revokes
    /// the user's entire refresh-token chain as a precaution against token theft.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<AuthenticatedUserResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserResponse>>> Refresh(RefreshTokenRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _authAppFunction.RefreshAsync(__request, GetClientIp(), __cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Token refreshed."));
    }

    /// <summary>Requests a password-reset email. Always returns the same generic success message whether
    /// or not the email is registered, so the response can never be used to enumerate accounts.</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> ForgotPassword(ForgotPasswordRequest __request, CancellationToken __cancellationToken)
    {
        await _authAppFunction.ForgotPasswordAsync(__request, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "If the email is registered, a password reset link has been sent."));
    }

    /// <summary>Completes a password reset using the token emailed by forgot-password. Fails if the
    /// token doesn't match the given email, was already used, or has expired; on success it also revokes
    /// every existing refresh token for the account (forces re-login everywhere).</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> ResetPassword(ResetPasswordRequest __request, CancellationToken __cancellationToken)
    {
        await _authAppFunction.ResetPasswordAsync(__request, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Password has been reset successfully."));
    }

    private string GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
