namespace OneClickYatra.Api.Models.Responses;

public sealed class EmailTemplateResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
