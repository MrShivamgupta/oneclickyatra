namespace OneClickYatra.Api.Models.Requests;

/// <summary>Full replacement of a role's permission set, by permission key (not Id — Ids are an
/// internal detail the frontend matrix UI never needs to know). Every key must already exist in
/// Permissions; unknown keys are rejected rather than silently ignored.</summary>
public sealed class UpdateRolePermissionsRequest
{
    public IReadOnlyList<string> PermissionKeys { get; set; } = Array.Empty<string>();
}
