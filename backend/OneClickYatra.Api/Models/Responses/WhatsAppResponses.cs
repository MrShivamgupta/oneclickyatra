namespace OneClickYatra.Api.Models.Responses;

public sealed class WhatsAppTemplateResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class NotificationLogResponse
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
    public Guid? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? GatewayMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
