using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

/// <summary>Search/filter shape for a future EmailTemplates admin screen -- only the repository
/// method is required for this pass, not the screen itself.</summary>
public sealed class EmailTemplateSearchRequest : PaginationRequest
{
    public bool? IsActive { get; set; }
}
