using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security.Password;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class UserAppFunction : IUserAppFunction
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<UserAppFunction> _logger;

    public UserAppFunction(
        IUserRepository __userRepository,
        IPasswordHasher __passwordHasher,
        IAuditLogWriter __auditLogWriter,
        ICurrentUserAccessor __currentUserAccessor,
        ILogger<UserAppFunction> __logger)
    {
        _userRepository = __userRepository;
        _passwordHasher = __passwordHasher;
        _auditLogWriter = __auditLogWriter;
        _currentUserAccessor = __currentUserAccessor;
        _logger = __logger;
    }

    public async Task<IReadOnlyList<UserSummaryResponse>> ListStaffAsync(CancellationToken __cancellationToken)
    {
        var users = await _userRepository.ListStaffAsync(__cancellationToken);
        return users.Select(u => new UserSummaryResponse { Id = u.Id, FullName = u.FullName, Email = u.Email }).ToList();
    }

    public async Task<PaginationResponse<UserResponse>> SearchAsync(UserSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _userRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<UserResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<UserResponse> CreateAsync(CreateStaffUserRequest __request, CancellationToken __cancellationToken)
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
        await _userRepository.AssignRoleAsync(user.Id, __request.Role, __cancellationToken);

        _logger.LogInformation("Staff user {UserId} created with role {Role} by {ActorUserId}", user.Id, __request.Role, _currentUserAccessor.UserId);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "user.created", "User", user.Id.ToString(), null, user.Email, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "user.created", "User", user.Id);
        }

        return await GetResponseAsync(user.Id, __cancellationToken);
    }

    public async Task<UserResponse> UpdateRoleAsync(Guid __id, UpdateUserRoleRequest __request, CancellationToken __cancellationToken)
    {
        _ = await _userRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("User", __id);

        var currentRoles = await _userRepository.GetRoleNamesAsync(__id, __cancellationToken);
        var oldRoleNames = currentRoles.Count > 0 ? string.Join(", ", currentRoles) : null;

        foreach (var roleName in currentRoles)
        {
            await _userRepository.RemoveRoleAsync(__id, roleName, __cancellationToken);
        }

        await _userRepository.AssignRoleAsync(__id, __request.Role, __cancellationToken);

        _logger.LogInformation("User {UserId} role changed {OldRoles} -> {NewRole} by {ActorUserId}", __id, oldRoleNames, __request.Role, _currentUserAccessor.UserId);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "user.role_changed", "User", __id.ToString(), oldRoleNames, __request.Role, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "user.role_changed", "User", __id);
        }

        return await GetResponseAsync(__id, __cancellationToken);
    }

    public async Task<UserResponse> UpdateStatusAsync(Guid __id, UpdateUserStatusRequest __request, CancellationToken __cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("User", __id);

        var oldIsActive = user.IsActive.ToString();
        await _userRepository.SetActiveStatusAsync(__id, __request.IsActive, __cancellationToken);

        _logger.LogInformation("User {UserId} status changed {OldIsActive} -> {NewIsActive} by {ActorUserId}", __id, oldIsActive, __request.IsActive, _currentUserAccessor.UserId);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "user.status_changed", "User", __id.ToString(), oldIsActive, __request.IsActive.ToString(), __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "user.status_changed", "User", __id);
        }

        return await GetResponseAsync(__id, __cancellationToken);
    }

    private async Task<UserResponse> GetResponseAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("User", __id);
        var roles = await _userRepository.GetRoleNamesAsync(__id, __cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt
        };
    }

    private static UserResponse ToResponse(UserModel __user) => new()
    {
        Id = __user.Id,
        FullName = __user.FullName,
        Email = __user.Email,
        IsActive = __user.IsActive,
        Roles = string.IsNullOrEmpty(__user.RoleNames)
            ? new List<string>()
            : __user.RoleNames.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
        CreatedAt = __user.CreatedAt
    };
}
