namespace OneClickYatra.Api.Models.Requests;

public sealed class PageRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool IsPublished { get; set; }
}
