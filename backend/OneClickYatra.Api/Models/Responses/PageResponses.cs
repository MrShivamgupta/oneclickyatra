namespace OneClickYatra.Api.Models.Responses;

public sealed class PageResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool IsPublished { get; set; }
}
