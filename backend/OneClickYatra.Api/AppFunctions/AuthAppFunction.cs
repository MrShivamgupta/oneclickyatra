using Microsoft.Extensions.Options;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security;
using OneClickYatra.Api.Security.Jwt;
using OneClickYatra.Api.Security.Password;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class AuthAppFunction : IAuthAppFunction
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly JwtOptions _jwtOptions;
    private readonly SecurityOptions _securityOptions;
    private readonly ILogger<AuthAppFunction> _logger;

    public AuthAppFunction(
        IUserRepository __userRepository,
        IRefreshTokenRepository __refreshTokenRepository,
        IPasswordResetTokenRepository __passwordResetTokenRepository,
        IPasswordHasher __passwordHasher,
        IJwtTokenService __jwtTokenService,
        IAuditLogWriter __auditLogWriter,
        IOptions<JwtOptions> __jwtOptions,
        IOptions<SecurityOptions> __securityOptions,
        ILogger<AuthAppFunction> __logger)
    {
        _userRepository = __userRepository;
        _refreshTokenRepository = __refreshTokenRepository;
        _passwordResetTokenRepository = __passwordResetTokenRepository;
        _passwordHasher = __passwordHasher;
        _jwtTokenService = __jwtTokenService;
        _auditLogWriter = __auditLogWriter;
        _jwtOptions = __jwtOptions.Value;
        _securityOptions = __securityOptions.Value;
        _logger = __logger;
    }

    public async Task<AuthenticatedUserResponse> LoginAsync(LoginRequest __request, string __ipAddress, CancellationToken __cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(__request.Email, __cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (user.LockedOutUntilUtc is { } lockedUntil && lockedUntil > DateTime.UtcNow)
        {
            throw new AccountLockedException(lockedUntil);
        }

        if (!user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        if (!_passwordHasher.Verify(__request.Password, user.PasswordHash))
        {
            await _userRepository.RecordFailedLoginAsync(
                user.Id,
                _securityOptions.MaxFailedLoginAttempts,
                TimeSpan.FromMinutes(_securityOptions.LockoutDurationMinutes),
                __cancellationToken);
            await _auditLogWriter.LogAsync(user.Id, "login.failed", "User", user.Id.ToString(), null, null, __cancellationToken);
            throw new InvalidCredentialsException();
        }

        await _userRepository.ResetFailedLoginAsync(user.Id, __cancellationToken);
        await _auditLogWriter.LogAsync(user.Id, "login.succeeded", "User", user.Id.ToString(), null, null, __cancellationToken);

        return await IssueTokensAsync(user, __ipAddress, __cancellationToken);
    }

    public async Task<AuthenticatedUserResponse> RegisterAsync(RegisterRequest __request, string __ipAddress, CancellationToken __cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(__request.Email, __cancellationToken);
        if (existingUser is not null)
        {
            throw new BusinessException("An account with this email already exists.");
        }

        var user = new UserModel
        {
            Id = Guid.NewGuid(),
            Email = __request.Email,
            FullName = __request.FullName,
            PasswordHash = _passwordHasher.Hash(__request.Password),
            IsActive = true
        };

        await _userRepository.CreateAsync(user, __cancellationToken);
        await _userRepository.AssignRoleAsync(user.Id, RoleConstants.Customer, __cancellationToken);
        await _auditLogWriter.LogAsync(user.Id, "user.registered", "User", user.Id.ToString(), null, user.Email, __cancellationToken);

        return await IssueTokensAsync(user, __ipAddress, __cancellationToken);
    }

    public async Task<AuthenticatedUserResponse> RefreshAsync(RefreshTokenRequest __request, string __ipAddress, CancellationToken __cancellationToken)
    {
        var tokenHash = TokenHasher.Hash(__request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, __cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!existingToken.IsActive)
        {
            // Reuse of a revoked/expired refresh token: revoke the whole chain for this user as a precaution.
            await _refreshTokenRepository.RevokeAllForUserAsync(existingToken.UserId, __cancellationToken);
            throw new InvalidCredentialsException();
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, __cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        var newRawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newTokenHash = TokenHasher.Hash(newRawRefreshToken);

        await _refreshTokenRepository.RevokeAsync(existingToken.Id, newTokenHash, __cancellationToken);

        return await IssueTokensAsync(user, __ipAddress, __cancellationToken, newRawRefreshToken, newTokenHash);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest __request, CancellationToken __cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(__request.Email, __cancellationToken);
        if (user is null)
        {
            // Do not reveal whether the email is registered.
            return;
        }

        var rawResetToken = _jwtTokenService.GenerateRefreshToken();
        var resetToken = new PasswordResetTokenModel
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawResetToken),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_securityOptions.PasswordResetTokenMinutes)
        };

        await _passwordResetTokenRepository.CreateAsync(resetToken, __cancellationToken);

        // Email delivery is wired up in the notifications phase; logged here so the flow is testable end-to-end today.
        _logger.LogInformation(
            "Password reset requested for {Email}. Reset token (dev-only visibility): {ResetToken}",
            user.Email, rawResetToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest __request, CancellationToken __cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(__request.Email, __cancellationToken)
            ?? throw new InvalidCredentialsException();

        var tokenHash = TokenHasher.Hash(__request.ResetToken);
        var resetToken = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, __cancellationToken);

        if (resetToken is null || resetToken.UserId != user.Id || resetToken.UsedAtUtc is not null || resetToken.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new BusinessException("This password reset link is invalid or has expired.");
        }

        await _userRepository.UpdatePasswordHashAsync(user.Id, _passwordHasher.Hash(__request.NewPassword), __cancellationToken);
        await _passwordResetTokenRepository.MarkUsedAsync(resetToken.Id, __cancellationToken);
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id, __cancellationToken);
        await _auditLogWriter.LogAsync(user.Id, "password.reset", "User", user.Id.ToString(), null, null, __cancellationToken);
    }

    private async Task<AuthenticatedUserResponse> IssueTokensAsync(
        UserModel __user,
        string __ipAddress,
        CancellationToken __cancellationToken,
        string? __precomputedRawRefreshToken = null,
        string? __precomputedRefreshTokenHash = null)
    {
        var roles = await _userRepository.GetRoleNamesAsync(__user.Id, __cancellationToken);
        var permissions = await _userRepository.GetPermissionKeysAsync(__user.Id, __cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(new JwtClaimsInput(__user.Id, __user.Email, __user.FullName, roles, permissions));

        var rawRefreshToken = __precomputedRawRefreshToken ?? _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = __precomputedRefreshTokenHash ?? TokenHasher.Hash(rawRefreshToken);

        await _refreshTokenRepository.CreateAsync(new RefreshTokenModel
        {
            Id = Guid.NewGuid(),
            UserId = __user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = __ipAddress
        }, __cancellationToken);

        return new AuthenticatedUserResponse
        {
            UserId = __user.Id,
            Email = __user.Email,
            FullName = __user.FullName,
            Roles = roles,
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = rawRefreshToken
        };
    }
}
