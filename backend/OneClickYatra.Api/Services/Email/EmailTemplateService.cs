using System.Text.RegularExpressions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.Services.Email;

public sealed class EmailTemplateService : IEmailTemplateService
{
    // Matches {{PlaceholderName}} tokens -- letters/digits/underscore only, same simple shape used
    // for every seeded template (see Seed015_EmailTemplates.sql).
    private static readonly Regex PlaceholderPattern = new("{{(\\w+)}}", RegexOptions.Compiled);

    private readonly IEmailTemplateRepository _templateRepository;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(IEmailTemplateRepository __templateRepository, ILogger<EmailTemplateService> __logger)
    {
        _templateRepository = __templateRepository;
        _logger = __logger;
    }

    public async Task<(string Subject, string Body)> RenderAsync(string __templateName, Dictionary<string, string> __placeholders, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByNameAsync(__templateName, __cancellationToken);
        if (template is null || !template.IsActive)
        {
            throw new EntityNotFoundException("EmailTemplate", __templateName);
        }

        var subject = ApplyPlaceholders(template.Subject, __placeholders, __templateName);
        var body = ApplyPlaceholders(template.BodyHtml, __placeholders, __templateName);

        return (subject, body);
    }

    /// <summary>Replaces every {{Key}} occurrence with its value from __placeholders. A token with
    /// no matching key is left as literal text -- logged as a warning so a missing optional field
    /// is visible without failing the send.</summary>
    private string ApplyPlaceholders(string __text, Dictionary<string, string> __placeholders, string __templateName)
    {
        return PlaceholderPattern.Replace(__text, match =>
        {
            var key = match.Groups[1].Value;
            if (__placeholders.TryGetValue(key, out var value))
            {
                return value;
            }

            _logger.LogWarning("Email template {TemplateName} has no placeholder value for {PlaceholderKey}; leaving the token as literal text.", __templateName, key);
            return match.Value;
        });
    }
}
