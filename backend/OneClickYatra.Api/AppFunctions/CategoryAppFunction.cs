using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class CategoryAppFunction : ICategoryAppFunction
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public CategoryAppFunction(ICategoryRepository __categoryRepository, ICurrentUserAccessor __currentUserAccessor, IAuditLogWriter __auditLogWriter)
    {
        _categoryRepository = __categoryRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    public async Task<PaginationResponse<CategoryResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _categoryRepository.ListAsync(__request, __cancellationToken);
        return PaginationResponse<CategoryResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Category", __id);
        return ToResponse(category);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest __request, CancellationToken __cancellationToken)
    {
        var existing = await _categoryRepository.GetBySlugAsync(__request.Slug, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A category with slug '{__request.Slug}' already exists.");
        }

        var category = new CategoryModel
        {
            Id = Guid.NewGuid(),
            Name = __request.Name,
            Slug = __request.Slug,
            Description = __request.Description,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _categoryRepository.CreateAsync(category, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "category.created", "Category", category.Id.ToString(), null, category.Name, __cancellationToken);

        return ToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid __id, CategoryRequest __request, CancellationToken __cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Category", __id);

        var oldName = category.Name;
        category.Name = __request.Name;
        category.Slug = __request.Slug;
        category.Description = __request.Description;
        category.UpdatedBy = _currentUserAccessor.UserId;

        await _categoryRepository.UpdateAsync(category, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "category.updated", "Category", category.Id.ToString(), oldName, category.Name, __cancellationToken);

        return ToResponse(category);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Category", __id);

        await _categoryRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "category.deleted", "Category", __id.ToString(), category.Name, null, __cancellationToken);
    }

    private static CategoryResponse ToResponse(CategoryModel category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        Description = category.Description
    };
}
