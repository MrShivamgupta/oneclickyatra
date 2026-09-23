using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class EmailTemplateSearchRequest : PaginationRequest
{
    public bool? IsActive { get; set; }
}

/// <summary>Shared shape for both create and update. Name is a real, code-referenced key (see
/// Seed015_EmailTemplates.sql's comment -- trigger-wiring AppFunctions call RenderAsync with these
/// exact names), so the frontend deliberately makes Name read-only once a template already exists
/// and only editable at creation time -- renaming a live template would silently break whichever
/// notification trigger looks it up by its old name.</summary>
public sealed class UpsertEmailTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
