namespace OneClickYatra.Api.Models.Responses;

public sealed class PermissionResponse
{
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class RolePermissionsResponse
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<string> PermissionKeys { get; set; } = Array.Empty<string>();
}

/// <summary>The whole role x permission grid in one payload — the "Roles and Permissions" admin page
/// renders this directly as a matrix (permissions as rows, roles as columns) rather than needing
/// separate roles/permissions/grants calls.</summary>
public sealed class PermissionMatrixResponse
{
    public IReadOnlyList<PermissionResponse> Permissions { get; set; } = Array.Empty<PermissionResponse>();
    public IReadOnlyList<RolePermissionsResponse> Roles { get; set; } = Array.Empty<RolePermissionsResponse>();
}
