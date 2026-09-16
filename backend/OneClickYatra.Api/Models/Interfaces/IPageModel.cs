namespace OneClickYatra.Api.Models.Interfaces;

public interface IPageModel
{
    Guid Id { get; }
    string Slug { get; }
    string Title { get; }
    string? Content { get; }
    bool IsPublished { get; }
    bool IsDeleted { get; }
}
