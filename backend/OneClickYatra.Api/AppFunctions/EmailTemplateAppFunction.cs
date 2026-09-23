using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>Admin CRUD over EmailTemplates -- the store EmailTemplateService.RenderAsync already
/// reads from to send every outbound notification email. This is the first thing that's ever
/// exposed it over HTTP; the repository/table/seed data all pre-date this by a wide margin.</summary>
public sealed class EmailTemplateAppFunction : IEmailTemplateAppFunction
{
    private readonly IEmailTemplateRepository _templateRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public EmailTemplateAppFunction(IEmailTemplateRepository __templateRepository, ICurrentUserAccessor __currentUserAccessor)
    {
        _templateRepository = __templateRepository;
        _currentUserAccessor = __currentUserAccessor;
    }

    public async Task<PaginationResponse<EmailTemplateResponse>> SearchAsync(EmailTemplateSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _templateRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<EmailTemplateResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<EmailTemplateResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("EmailTemplate", __id);
        return ToResponse(template);
    }

    public async Task<EmailTemplateResponse> CreateAsync(UpsertEmailTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var existing = await _templateRepository.GetByNameAsync(__request.Name, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A template named '{__request.Name}' already exists.");
        }

        var template = new EmailTemplateModel
        {
            Id = Guid.NewGuid(),
            Name = __request.Name,
            Subject = __request.Subject,
            BodyHtml = __request.BodyHtml,
            IsActive = __request.IsActive,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _templateRepository.CreateAsync(template, __cancellationToken);
        return await GetByIdAsync(template.Id, __cancellationToken);
    }

    public async Task<EmailTemplateResponse> UpdateAsync(Guid __id, UpsertEmailTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("EmailTemplate", __id);

        // Name is deliberately NOT updated here even if the request carries a different value -- see
        // UpsertEmailTemplateRequest's doc comment. The frontend's edit form disables the field for
        // exactly this reason; this is the server-side backstop in case that's ever bypassed.
        template.Subject = __request.Subject;
        template.BodyHtml = __request.BodyHtml;
        template.IsActive = __request.IsActive;
        template.UpdatedBy = _currentUserAccessor.UserId;

        await _templateRepository.UpdateAsync(template, __cancellationToken);
        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        _ = await _templateRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("EmailTemplate", __id);
        await _templateRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
    }

    private static EmailTemplateResponse ToResponse(EmailTemplateModel template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Subject = template.Subject,
        BodyHtml = template.BodyHtml,
        IsActive = template.IsActive,
        UpdatedAt = template.UpdatedAt
    };
}
