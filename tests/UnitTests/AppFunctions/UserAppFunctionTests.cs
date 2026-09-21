using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.AppValidations;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security.Password;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class UserAppFunctionTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<ILogger<UserAppFunction>> _logger = new();

    private UserAppFunction CreateSut() => new(
        _userRepository.Object,
        _passwordHasher.Object,
        _auditLogWriter.Object,
        _currentUserAccessor.Object,
        _logger.Object);

    private static UserModel CreateUser(bool isActive = true) => new()
    {
        Id = Guid.NewGuid(),
        Email = "agent@example.com",
        FullName = "Test Agent",
        PasswordHash = "hashed-password",
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task CreateAsync_NewEmail_CreatesUserHashesPasswordAndAssignsRole()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("new.agent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        _passwordHasher.Setup(h => h.Hash("Passw0rd1")).Returns("hashed-password");

        Guid? createdUserId = null;
        _userRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()))
            .Callback<UserModel, CancellationToken>((user, _) => createdUserId = user.Id)
            .ReturnsAsync(Guid.NewGuid());
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserModel
            {
                Id = Guid.NewGuid(),
                Email = "new.agent@example.com",
                FullName = "New Agent",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        _userRepository
            .Setup(r => r.GetRoleNamesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([RoleConstants.TravelAgent]);

        var sut = CreateSut();
        var request = new CreateStaffUserRequest
        {
            FullName = "New Agent",
            Email = "new.agent@example.com",
            Password = "Passw0rd1",
            Role = RoleConstants.TravelAgent
        };

        var result = await sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal("new.agent@example.com", result.Email);
        Assert.Contains(RoleConstants.TravelAgent, result.Roles);
        _userRepository.Verify(r => r.CreateAsync(It.Is<UserModel>(u =>
            u.Email == "new.agent@example.com" && u.PasswordHash == "hashed-password" && u.IsActive), It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.AssignRoleAsync(It.IsAny<Guid>(), RoleConstants.TravelAgent, It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(w => w.LogAsync(
            It.IsAny<Guid?>(), "user.created", "User", createdUserId!.Value.ToString(), null, "new.agent@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmailAlreadyExists_ThrowsBusinessException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser());

        var sut = CreateSut();
        var request = new CreateStaffUserRequest
        {
            FullName = "Existing",
            Email = "existing@example.com",
            Password = "Passw0rd1",
            Role = RoleConstants.TravelAgent
        };

        await Assert.ThrowsAsync<BusinessException>(() => sut.CreateAsync(request, CancellationToken.None));
        _userRepository.Verify(r => r.CreateAsync(It.IsAny<UserModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void CreateAsync_DisallowedRole_FailsValidation()
    {
        var validator = new CreateStaffUserRequestValidator();
        var request = new CreateStaffUserRequest
        {
            FullName = "Someone",
            Email = "someone@example.com",
            Password = "Passw0rd1",
            Role = RoleConstants.Customer
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateStaffUserRequest.Role));
    }

    [Fact]
    public void CreateAsync_AllowedRole_PassesRoleValidation()
    {
        var validator = new CreateStaffUserRequestValidator();
        var request = new CreateStaffUserRequest
        {
            FullName = "Someone",
            Email = "someone@example.com",
            Password = "Passw0rd1",
            Role = RoleConstants.OperationsStaff
        };

        var result = validator.Validate(request);

        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(CreateStaffUserRequest.Role));
    }

    [Fact]
    public async Task UpdateRoleAsync_ExistingUser_RemovesOldRoleAssignsNewAndAudits()
    {
        var user = CreateUser();
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepository
            .SetupSequence(r => r.GetRoleNamesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([RoleConstants.TravelAgent])
            .ReturnsAsync([RoleConstants.Finance]);

        var sut = CreateSut();
        var request = new UpdateUserRoleRequest { Role = RoleConstants.Finance };

        var result = await sut.UpdateRoleAsync(user.Id, request, CancellationToken.None);

        Assert.Contains(RoleConstants.Finance, result.Roles);
        _userRepository.Verify(r => r.RemoveRoleAsync(user.Id, RoleConstants.TravelAgent, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.AssignRoleAsync(user.Id, RoleConstants.Finance, It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(w => w.LogAsync(
            It.IsAny<Guid?>(), "user.role_changed", "User", user.Id.ToString(), RoleConstants.TravelAgent, RoleConstants.Finance, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_UnknownUserId_ThrowsEntityNotFoundException()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var sut = CreateSut();
        var request = new UpdateUserRoleRequest { Role = RoleConstants.Finance };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.UpdateRoleAsync(Guid.NewGuid(), request, CancellationToken.None));
        _userRepository.Verify(r => r.AssignRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ExistingUser_TogglesStatusAndAudits()
    {
        var user = CreateUser(isActive: true);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepository.Setup(r => r.GetRoleNamesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([RoleConstants.TravelAgent]);

        var sut = CreateSut();
        var request = new UpdateUserStatusRequest { IsActive = false };

        await sut.UpdateStatusAsync(user.Id, request, CancellationToken.None);

        _userRepository.Verify(r => r.SetActiveStatusAsync(user.Id, false, It.IsAny<CancellationToken>()), Times.Once);
        _auditLogWriter.Verify(w => w.LogAsync(
            It.IsAny<Guid?>(), "user.status_changed", "User", user.Id.ToString(), "True", "False", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_UnknownUserId_ThrowsEntityNotFoundException()
    {
        _userRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);

        var sut = CreateSut();
        var request = new UpdateUserStatusRequest { IsActive = false };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.UpdateStatusAsync(Guid.NewGuid(), request, CancellationToken.None));
        _userRepository.Verify(r => r.SetActiveStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
