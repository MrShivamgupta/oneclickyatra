namespace OneClickYatra.Api.Models.Interfaces;

public interface IRoleModel
{
    Guid Id { get; }
    string Name { get; }
    string? Description { get; }
    bool IsDeleted { get; }
}
