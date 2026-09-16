namespace OneClickYatra.Api.Models.Interfaces;

public interface ICategoryModel
{
    Guid Id { get; }
    string Name { get; }
    string Slug { get; }
    string? Description { get; }
    bool IsDeleted { get; }
}
