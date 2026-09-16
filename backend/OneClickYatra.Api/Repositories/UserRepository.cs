using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<UserModel?> GetByEmailAsync(string __email, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Email, FullName, PasswordHash, IsActive, FailedLoginAttempts, LockedOutUntilUtc,
                   CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Users
            WHERE Email = @Email AND IsDeleted = 0
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Email = __email }, cancellationToken: __cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserModel>(command);
    }

    public async Task<UserModel?> GetByIdAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Email, FullName, PasswordHash, IsActive, FailedLoginAttempts, LockedOutUntilUtc,
                   CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
            FROM Users
            WHERE Id = @UserId AND IsDeleted = 0
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId }, cancellationToken: __cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UserModel>(command);
    }

    public async Task<Guid> CreateAsync(UserModel __user, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive, FailedLoginAttempts, CreatedAt, IsDeleted)
            VALUES (@Id, @Email, @FullName, @PasswordHash, @IsActive, 0, SYSUTCDATETIME(), 0)
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            __user.Id,
            __user.Email,
            __user.FullName,
            __user.PasswordHash,
            __user.IsActive
        }, cancellationToken: __cancellationToken);

        await connection.ExecuteAsync(command);
        return __user.Id;
    }

    public async Task<IReadOnlyList<UserModel>> ListStaffAsync(CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT u.Id, u.Email, u.FullName, u.IsActive
            FROM Users u
            INNER JOIN UserRoles ur ON ur.UserId = u.Id
            INNER JOIN Roles r ON r.Id = ur.RoleId AND r.IsDeleted = 0
            WHERE u.IsDeleted = 0 AND u.IsActive = 1
              AND r.Name IN ('TravelAgent', 'OperationsStaff', 'Finance', 'SuperAdmin')
            ORDER BY u.FullName
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: __cancellationToken);
        var users = await connection.QueryAsync<UserModel>(command);
        return users.ToList();
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT r.Name
            FROM UserRoles ur
            INNER JOIN Roles r ON r.Id = ur.RoleId AND r.IsDeleted = 0
            WHERE ur.UserId = @UserId
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId }, cancellationToken: __cancellationToken);
        var roles = await connection.QueryAsync<string>(command);
        return roles.ToList();
    }

    public async Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT p.[Key]
            FROM UserRoles ur
            INNER JOIN RolePermissions rp ON rp.RoleId = ur.RoleId
            INNER JOIN Permissions p ON p.Id = rp.PermissionId AND p.IsDeleted = 0
            WHERE ur.UserId = @UserId
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId }, cancellationToken: __cancellationToken);
        var permissions = await connection.QueryAsync<string>(command);
        return permissions.ToList();
    }

    public async Task AssignRoleAsync(Guid __userId, string __roleName, CancellationToken __cancellationToken)
    {
        const string sql = """
            INSERT INTO UserRoles (UserId, RoleId, CreatedAt)
            SELECT @UserId, r.Id, SYSUTCDATETIME()
            FROM Roles r
            WHERE r.Name = @RoleName AND r.IsDeleted = 0
              AND NOT EXISTS (
                  SELECT 1 FROM UserRoles existing WHERE existing.UserId = @UserId AND existing.RoleId = r.Id
              )
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId, RoleName = __roleName }, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task RecordFailedLoginAsync(Guid __userId, int __maxAttempts, TimeSpan __lockoutDuration, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Users
            SET FailedLoginAttempts = FailedLoginAttempts + 1,
                LockedOutUntilUtc = CASE
                    WHEN FailedLoginAttempts + 1 >= @MaxAttempts THEN DATEADD(MINUTE, @LockoutMinutes, SYSUTCDATETIME())
                    ELSE LockedOutUntilUtc
                END,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new
        {
            UserId = __userId,
            MaxAttempts = __maxAttempts,
            LockoutMinutes = __lockoutDuration.TotalMinutes
        }, cancellationToken: __cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task ResetFailedLoginAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Users
            SET FailedLoginAttempts = 0, LockedOutUntilUtc = NULL, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId }, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task UpdatePasswordHashAsync(Guid __userId, string __passwordHash, CancellationToken __cancellationToken)
    {
        const string sql = """
            UPDATE Users
            SET PasswordHash = @PasswordHash, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { UserId = __userId, PasswordHash = __passwordHash }, cancellationToken: __cancellationToken);
        await connection.ExecuteAsync(command);
    }
}
