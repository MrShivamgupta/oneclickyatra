using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class PageAppFunction : IPageAppFunction
{
    private readonly IPageRepository _pageRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public PageAppFunction(IPageRepository __pageRepository, ICurrentUserAccessor __currentUserAccessor, IAuditLogWriter __auditLogWriter)
    {
        _pageRepository = __pageRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    /// <summary>Public lookup used by the anonymous site renderer. Never leaks unpublished pages.</summary>
    public async Task<PageResponse> GetPublishedBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        var page = await _pageRepository.GetBySlugAsync(__slug, __cancellationToken);
        if (page is null || !page.IsPublished)
        {
            throw new EntityNotFoundException("Page", __slug);
        }

        return ToResponse(page);
    }

    public async Task<PaginationResponse<PageResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _pageRepository.ListAsync(__request, __cancellationToken);
        return PaginationResponse<PageResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<PageResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Page", __id);
        return ToResponse(page);
    }

    public async Task<PageResponse> CreateAsync(PageRequest __request, CancellationToken __cancellationToken)
    {
        var existing = await _pageRepository.GetBySlugAsync(__request.Slug, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A page with slug '{__request.Slug}' already exists.");
        }

        var page = new PageModel
        {
            Id = Guid.NewGuid(),
            Slug = __request.Slug,
            Title = __request.Title,
            Content = __request.Content,
            IsPublished = __request.IsPublished,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _pageRepository.CreateAsync(page, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "cms.page.created", "Page", page.Id.ToString(), null, page.Title, __cancellationToken);

        return ToResponse(page);
    }

    public async Task<PageResponse> UpdateAsync(Guid __id, PageRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Page", __id);

        var existingWithSlug = await _pageRepository.GetBySlugAsync(__request.Slug, __cancellationToken);
        if (existingWithSlug is not null && existingWithSlug.Id != __id)
        {
            throw new BusinessException($"A page with slug '{__request.Slug}' already exists.");
        }

        var oldTitle = page.Title;
        page.Slug = __request.Slug;
        page.Title = __request.Title;
        page.Content = __request.Content;
        page.IsPublished = __request.IsPublished;
        page.UpdatedBy = _currentUserAccessor.UserId;

        await _pageRepository.UpdateAsync(page, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "cms.page.updated", "Page", page.Id.ToString(), oldTitle, page.Title, __cancellationToken);

        return ToResponse(page);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Page", __id);

        await _pageRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "cms.page.deleted", "Page", __id.ToString(), page.Title, null, __cancellationToken);
    }

    private static PageResponse ToResponse(PageModel page) => new()
    {
        Id = page.Id,
        Slug = page.Slug,
        Title = page.Title,
        Content = page.Content,
        IsPublished = page.IsPublished
    };
}
