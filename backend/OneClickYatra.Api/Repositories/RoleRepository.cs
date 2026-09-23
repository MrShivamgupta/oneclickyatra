using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<IReadOnlyList<RoleModel>> GetAllRolesAsync(CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, Description
            FROM Roles
            WHERE IsDeleted = 0
            ORDER BY Name
            """;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<RoleModel>(new CommandDefinition(sql, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<IReadOnlyList<PermissionModel>> GetAllPermissionsAsync(CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, [Key], Description
            FROM Permissions
            WHERE IsDeleted = 0
            ORDER BY [Key]
            """;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PermissionModel>(new CommandDefinition(sql, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<IReadOnlyList<(Guid RoleId, string PermissionKey)>> GetAllGrantsAsync(CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT rp.RoleId, p.[Key] AS PermissionKey
            FROM RolePermissions rp
            INNER JOIN Permissions p ON p.Id = rp.PermissionId AND p.IsDeleted = 0
            """;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<(Guid RoleId, string PermissionKey)>(new CommandDefinition(sql, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task<RoleModel?> GetRoleByIdAsync(Guid __roleId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, Name, Description
            FROM Roles
            WHERE Id = @RoleId AND IsDeleted = 0
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { RoleId = __roleId }, cancellationToken: __cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<RoleModel>(command);
    }

    public async Task<IReadOnlyList<PermissionModel>> GetPermissionsByKeysAsync(IReadOnlyList<string> __keys, CancellationToken __cancellationToken)
    {
        if (__keys.Count == 0) return Array.Empty<PermissionModel>();

        const string sql = """
            SELECT Id, [Key], Description
            FROM Permissions
            WHERE [Key] IN @Keys AND IsDeleted = 0
            """;

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Keys = __keys }, cancellationToken: __cancellationToken);
        var result = await connection.QueryAsync<PermissionModel>(command);
        return result.ToList();
    }

    public async Task ReplaceRolePermissionsAsync(Guid __roleId, IReadOnlyList<Guid> __permissionIds, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM RolePermissions WHERE RoleId = @RoleId",
            new { RoleId = __roleId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO RolePermissions (RoleId, PermissionId, CreatedAt)
            VALUES (@RoleId, @PermissionId, SYSUTCDATETIME())
            """;
        foreach (var permissionId in __permissionIds)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                RoleId = __roleId,
                PermissionId = permissionId
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }
}
