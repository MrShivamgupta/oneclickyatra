using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class CountryAppFunction : ICountryAppFunction
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<CountryAppFunction> _logger;

    public CountryAppFunction(
        ICountryRepository __countryRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ILogger<CountryAppFunction> __logger)
    {
        _countryRepository = __countryRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<PaginationResponse<CountryResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _countryRepository.ListAsync(__request, __cancellationToken);
        return PaginationResponse<CountryResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<CountryResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var country = await _countryRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Country", __id);
        return ToResponse(country);
    }

    public async Task<CountryResponse> CreateAsync(CountryRequest __request, CancellationToken __cancellationToken)
    {
        var existing = await _countryRepository.GetByNameAsync(__request.Name, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A country named '{__request.Name}' already exists.");
        }

        var country = new CountryModel
        {
            Id = Guid.NewGuid(),
            Name = __request.Name,
            IsoCode = __request.IsoCode.ToUpperInvariant(),
            CreatedBy = _currentUserAccessor.UserId
        };

        await _countryRepository.CreateAsync(country, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "country.created", "Country", country.Id.ToString(), null, country.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "country.created", "Country", country.Id);
        }

        _logger.LogInformation("Country {CountryId} {CountryName} created by {UserId}", country.Id, country.Name, _currentUserAccessor.UserId);

        return ToResponse(country);
    }

    public async Task<CountryResponse> UpdateAsync(Guid __id, CountryRequest __request, CancellationToken __cancellationToken)
    {
        var country = await _countryRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Country", __id);

        var oldName = country.Name;
        country.Name = __request.Name;
        country.IsoCode = __request.IsoCode.ToUpperInvariant();
        country.UpdatedBy = _currentUserAccessor.UserId;

        await _countryRepository.UpdateAsync(country, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "country.updated", "Country", country.Id.ToString(), oldName, country.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "country.updated", "Country", country.Id);
        }

        _logger.LogInformation("Country {CountryId} updated {OldName} -> {NewName} by {UserId}", country.Id, oldName, country.Name, _currentUserAccessor.UserId);

        return ToResponse(country);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var country = await _countryRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Country", __id);

        await _countryRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "country.deleted", "Country", __id.ToString(), country.Name, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "country.deleted", "Country", __id);
        }

        _logger.LogInformation("Country {CountryId} {CountryName} deleted by {UserId}", __id, country.Name, _currentUserAccessor.UserId);
    }

    private static CountryResponse ToResponse(CountryModel country) => new()
    {
        Id = country.Id,
        Name = country.Name,
        IsoCode = country.IsoCode
    };
}
