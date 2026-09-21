namespace OneClickYatra.Api.Models;

public sealed class NotificationLogModel
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = "WhatsApp";
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
    public Guid? TemplateId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Status { get; set; } = "Queued";
    public string? GatewayMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Joined in by the repository for display; not a real column on this table.</summary>
    public string? TemplateName { get; set; }
}
