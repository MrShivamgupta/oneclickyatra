using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class CityAppFunction : ICityAppFunction
{
    private readonly ICityRepository _cityRepository;
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<CityAppFunction> _logger;

    public CityAppFunction(
        ICityRepository __cityRepository,
        ICountryRepository __countryRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ILogger<CityAppFunction> __logger)
    {
        _cityRepository = __cityRepository;
        _countryRepository = __countryRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<PaginationResponse<CityResponse>> ListAsync(PaginationRequest __request, Guid? __countryId, CancellationToken __cancellationToken)
    {
        var page = await _cityRepository.ListAsync(__request, __countryId, __cancellationToken);
        return PaginationResponse<CityResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<CityResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var city = await _cityRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("City", __id);
        return ToResponse(city);
    }

    public async Task<CityResponse> CreateAsync(CityRequest __request, CancellationToken __cancellationToken)
    {
        await EnsureCountryExistsAsync(__request.CountryId, __cancellationToken);

        var existing = await _cityRepository.GetByCountryAndNameAsync(__request.CountryId, __request.Name, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A city named '{__request.Name}' already exists for this country.");
        }

        var city = new CityModel
        {
            Id = Guid.NewGuid(),
            CountryId = __request.CountryId,
            Name = __request.Name,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _cityRepository.CreateAsync(city, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "city.created", "City", city.Id.ToString(), null, city.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "city.created", "City", city.Id);
        }

        _logger.LogInformation("City {CityId} {CityName} created by {UserId}", city.Id, city.Name, _currentUserAccessor.UserId);

        return await GetByIdAsync(city.Id, __cancellationToken);
    }

    public async Task<CityResponse> UpdateAsync(Guid __id, CityRequest __request, CancellationToken __cancellationToken)
    {
        var city = await _cityRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("City", __id);
        await EnsureCountryExistsAsync(__request.CountryId, __cancellationToken);

        var oldName = city.Name;
        city.CountryId = __request.CountryId;
        city.Name = __request.Name;
        city.UpdatedBy = _currentUserAccessor.UserId;

        await _cityRepository.UpdateAsync(city, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "city.updated", "City", city.Id.ToString(), oldName, city.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "city.updated", "City", city.Id);
        }

        _logger.LogInformation("City {CityId} updated {OldName} -> {NewName} by {UserId}", city.Id, oldName, city.Name, _currentUserAccessor.UserId);

        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var city = await _cityRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("City", __id);

        await _cityRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "city.deleted", "City", __id.ToString(), city.Name, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "city.deleted", "City", __id);
        }

        _logger.LogInformation("City {CityId} {CityName} deleted by {UserId}", __id, city.Name, _currentUserAccessor.UserId);
    }

    private async Task EnsureCountryExistsAsync(Guid __countryId, CancellationToken __cancellationToken)
    {
        _ = await _countryRepository.GetByIdAsync(__countryId, __cancellationToken)
            ?? throw new EntityNotFoundException("Country", __countryId);
    }

    private static CityResponse ToResponse(CityModel city) => new()
    {
        Id = city.Id,
        CountryId = city.CountryId,
        CountryName = city.CountryName ?? string.Empty,
        Name = city.Name
    };
}
