using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class WhatsAppTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class WhatsAppTemplateSearchRequest : PaginationRequest
{
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Sends a pre-approved Meta template message — free-form text cannot be sent outside the
/// 24-hour customer-service window, so every outbound send goes through a named template.</summary>
public sealed class WhatsAppSendMessageRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string[] Parameters { get; set; } = [];
}

public sealed class NotificationLogSearchRequest : PaginationRequest
{
    public string? Channel { get; set; }
    public string? Status { get; set; }
}
