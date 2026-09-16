using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class DestinationAppFunction : IDestinationAppFunction
{
    private const string CacheKeyPrefix = "destination:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly IDestinationRepository _destinationRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICacheService _cacheService;

    public DestinationAppFunction(
        IDestinationRepository __destinationRepository,
        ICountryRepository __countryRepository,
        ICityRepository __cityRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ICacheService __cacheService)
    {
        _destinationRepository = __destinationRepository;
        _countryRepository = __countryRepository;
        _cityRepository = __cityRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _cacheService = __cacheService;
    }

    public async Task<PaginationResponse<DestinationResponse>> SearchAsync(DestinationSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _destinationRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<DestinationResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<DestinationResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}id:{__id}";
        var cached = await _cacheService.GetAsync<DestinationResponse>(cacheKey, __cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var destination = await _destinationRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Destination", __id);

        var response = ToResponse(destination);
        await _cacheService.SetAsync(cacheKey, response, CacheTtl, __cancellationToken);
        return response;
    }

    /// <summary>Public: used for pretty public-site URLs like /destinations/goa-india. Never exposes unpublished content.</summary>
    public async Task<DestinationResponse> GetBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}slug:{__slug}";
        var cached = await _cacheService.GetAsync<DestinationResponse>(cacheKey, __cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var destination = await _destinationRepository.GetBySlugAsync(__slug, __cancellationToken);
        if (destination is null || !destination.IsPublished)
        {
            throw new EntityNotFoundException("Destination", __slug);
        }

        var response = ToResponse(destination);
        await _cacheService.SetAsync(cacheKey, response, CacheTtl, __cancellationToken);
        return response;
    }

    public async Task<DestinationResponse> CreateAsync(DestinationRequest __request, CancellationToken __cancellationToken)
    {
        await ValidateReferencesAsync(__request, __cancellationToken);

        var existing = await _destinationRepository.GetBySlugAsync(__request.Slug, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A destination with slug '{__request.Slug}' already exists.");
        }

        var destination = new DestinationModel
        {
            Id = Guid.NewGuid(),
            CountryId = __request.CountryId,
            CityId = __request.CityId,
            Name = __request.Name,
            Slug = __request.Slug,
            ShortDescription = __request.ShortDescription,
            Description = __request.Description,
            HeroImageUrl = __request.HeroImageUrl,
            IsFeatured = __request.IsFeatured,
            IsPublished = __request.IsPublished,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _destinationRepository.CreateAsync(destination, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "destination.created", "Destination", destination.Id.ToString(), null, destination.Name, __cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix, __cancellationToken);

        return await GetByIdAsync(destination.Id, __cancellationToken);
    }

    public async Task<DestinationResponse> UpdateAsync(Guid __id, DestinationRequest __request, CancellationToken __cancellationToken)
    {
        var destination = await _destinationRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Destination", __id);
        await ValidateReferencesAsync(__request, __cancellationToken);

        var oldName = destination.Name;
        destination.CountryId = __request.CountryId;
        destination.CityId = __request.CityId;
        destination.Name = __request.Name;
        destination.Slug = __request.Slug;
        destination.ShortDescription = __request.ShortDescription;
        destination.Description = __request.Description;
        destination.HeroImageUrl = __request.HeroImageUrl;
        destination.IsFeatured = __request.IsFeatured;
        destination.IsPublished = __request.IsPublished;
        destination.UpdatedBy = _currentUserAccessor.UserId;

        await _destinationRepository.UpdateAsync(destination, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "destination.updated", "Destination", destination.Id.ToString(), oldName, destination.Name, __cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix, __cancellationToken);

        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var destination = await _destinationRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Destination", __id);

        await _destinationRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "destination.deleted", "Destination", __id.ToString(), destination.Name, null, __cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix, __cancellationToken);
    }

    private async Task ValidateReferencesAsync(DestinationRequest __request, CancellationToken __cancellationToken)
    {
        _ = await _countryRepository.GetByIdAsync(__request.CountryId, __cancellationToken)
            ?? throw new EntityNotFoundException("Country", __request.CountryId);

        if (__request.CityId is { } cityId)
        {
            var city = await _cityRepository.GetByIdAsync(cityId, __cancellationToken)
                ?? throw new EntityNotFoundException("City", cityId);

            if (city.CountryId != __request.CountryId)
            {
                throw new BusinessException("The selected city does not belong to the selected country.");
            }
        }
    }

    private static DestinationResponse ToResponse(DestinationModel destination) => new()
    {
        Id = destination.Id,
        CountryId = destination.CountryId,
        CountryName = destination.CountryName ?? string.Empty,
        CityId = destination.CityId,
        CityName = destination.CityName,
        Name = destination.Name,
        Slug = destination.Slug,
        ShortDescription = destination.ShortDescription,
        Description = destination.Description,
        HeroImageUrl = destination.HeroImageUrl,
        IsFeatured = destination.IsFeatured,
        IsPublished = destination.IsPublished
    };
}
