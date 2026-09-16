using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security;
using OneClickYatra.Api.Security.Jwt;
using OneClickYatra.Api.Security.Password;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class AuthAppFunctionTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private AuthAppFunction CreateSut(int maxFailedAttempts = 5, int lockoutMinutes = 15)
    {
        _jwtTokenService
            .Setup(service => service.GenerateAccessToken(It.IsAny<JwtClaimsInput>()))
            .Returns(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenService
            .Setup(service => service.GenerateRefreshToken())
            .Returns("raw-refresh-token");

        return new AuthAppFunction(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _passwordResetTokenRepository.Object,
            _passwordHasher.Object,
            _jwtTokenService.Object,
            _auditLogWriter.Object,
            Options.Create(new JwtOptions { Issuer = "test", Audience = "test", Key = "test-key", RefreshTokenDays = 7 }),
            Options.Create(new SecurityOptions { MaxFailedLoginAttempts = maxFailedAttempts, LockoutDurationMinutes = lockoutMinutes }),
            Mock.Of<ILogger<AuthAppFunction>>());
    }

    private static UserModel CreateUser(bool isActive = true, DateTime? lockedOutUntilUtc = null) => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        FullName = "Test User",
        PasswordHash = "hashed-password",
        IsActive = isActive,
        LockedOutUntilUtc = lockedOutUntilUtc
    };

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentials()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.LoginAsync(new LoginRequest { Email = "missing@example.com", Password = "x" }, "127.0.0.1", CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_AccountLockedInFuture_ThrowsAccountLocked()
    {
        var user = CreateUser(lockedOutUntilUtc: DateTime.UtcNow.AddMinutes(5));
        _userRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var sut = CreateSut();

        await Assert.ThrowsAsync<AccountLockedException>(() =>
            sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "anything" }, "127.0.0.1", CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_RecordsFailedAttemptAndThrows()
    {
        var user = CreateUser();
        _userRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong", user.PasswordHash)).Returns(false);

        var sut = CreateSut(maxFailedAttempts: 5, lockoutMinutes: 15);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "wrong" }, "127.0.0.1", CancellationToken.None));

        _userRepository.Verify(r => r.RecordFailedLoginAsync(user.Id, 5, TimeSpan.FromMinutes(15), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ResetsFailedAttemptsAndReturnsTokens()
    {
        var user = CreateUser();
        _userRepository.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("correct", user.PasswordHash)).Returns(true);
        _userRepository.Setup(r => r.GetRoleNamesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["TravelAgent"]);
        _userRepository.Setup(r => r.GetPermissionKeysAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["lead.view"]);

        var sut = CreateSut();

        var result = await sut.LoginAsync(new LoginRequest { Email = user.Email, Password = "correct" }, "127.0.0.1", CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        Assert.Contains("TravelAgent", result.Roles);
        _userRepository.Verify(r => r.ResetFailedLoginAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.CreateAsync(It.Is<RefreshTokenModel>(t => t.UserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsBusinessException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser());

        var sut = CreateSut();
        var request = new RegisterRequest { Email = "existing@example.com", FullName = "X", Password = "Passw0rd1", Phone = "+911234567890" };

        await Assert.ThrowsAsync<BusinessException>(() => sut.RegisterAsync(request, "127.0.0.1", CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndAssignsCustomerRole()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        var sut = CreateSut();
        var request = new RegisterRequest { Email = "new@example.com", FullName = "New User", Password = "Passw0rd1", Phone = "+911234567890" };

        var result = await sut.RegisterAsync(request, "127.0.0.1", CancellationToken.None);

        Assert.Equal("new@example.com", result.Email);
        _userRepository.Verify(r => r.CreateAsync(It.Is<UserModel>(u => u.Email == "new@example.com"), It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.AssignRoleAsync(It.IsAny<Guid>(), RoleConstants.Customer, It.IsAny<CancellationToken>()), Times.Once);
    }
}
