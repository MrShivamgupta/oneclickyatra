using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IAuthAppFunction
{
    Task<AuthenticatedUserResponse> LoginAsync(LoginRequest __request, string __ipAddress, CancellationToken __cancellationToken);
    Task<AuthenticatedUserResponse> RegisterAsync(RegisterRequest __request, string __ipAddress, CancellationToken __cancellationToken);
    Task<AuthenticatedUserResponse> RefreshAsync(RefreshTokenRequest __request, string __ipAddress, CancellationToken __cancellationToken);
    Task ForgotPasswordAsync(ForgotPasswordRequest __request, CancellationToken __cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequest __request, CancellationToken __cancellationToken);
}
