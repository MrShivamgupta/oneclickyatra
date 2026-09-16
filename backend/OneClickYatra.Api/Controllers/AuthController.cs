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

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserResponse>>> Login(LoginRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _authAppFunction.LoginAsync(__request, GetClientIp(), __cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Login successful."));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserResponse>>> Register(RegisterRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _authAppFunction.RegisterAsync(__request, GetClientIp(), __cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Registration successful."));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserResponse>>> Refresh(RefreshTokenRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _authAppFunction.RefreshAsync(__request, GetClientIp(), __cancellationToken);
        return Ok(ApiResponse<AuthenticatedUserResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Token refreshed."));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ForgotPassword(ForgotPasswordRequest __request, CancellationToken __cancellationToken)
    {
        await _authAppFunction.ForgotPasswordAsync(__request, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "If the email is registered, a password reset link has been sent."));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object?>>> ResetPassword(ResetPasswordRequest __request, CancellationToken __cancellationToken)
    {
        await _authAppFunction.ResetPasswordAsync(__request, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Password has been reset successfully."));
    }

    private string GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
