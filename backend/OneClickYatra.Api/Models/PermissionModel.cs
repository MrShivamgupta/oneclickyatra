namespace OneClickYatra.Api.Models;

public sealed class PermissionModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
