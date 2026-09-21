namespace OneClickYatra.Api.Services.Email;

/// <summary>
/// Renders a named, active EmailTemplates row by substituting {{PlaceholderName}} tokens with
/// supplied values -- simple literal string replacement, not Handlebars/Razor.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>Looks up the named template (throws <see cref="OneClickYatra.Api.Globals.EntityNotFoundException"/>
    /// if it does not exist or is inactive) and replaces every {{Key}} occurrence with its value
    /// from __placeholders. A placeholder token with no matching key in __placeholders is left in
    /// the rendered text as literal {{Key}} rather than throwing -- a missing optional field should
    /// not break the whole send.</summary>
    Task<(string Subject, string Body)> RenderAsync(string __templateName, Dictionary<string, string> __placeholders, CancellationToken __cancellationToken);
}
