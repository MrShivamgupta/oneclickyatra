namespace OneClickYatra.Api.Models;

public sealed class PermissionModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
}
