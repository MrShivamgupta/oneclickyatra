using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class SeasonAppFunction : ISeasonAppFunction
{
    private readonly ISeasonRepository _seasonRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public SeasonAppFunction(ISeasonRepository __seasonRepository, ICurrentUserAccessor __currentUserAccessor, IAuditLogWriter __auditLogWriter)
    {
        _seasonRepository = __seasonRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    public async Task<PaginationResponse<SeasonResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _seasonRepository.ListAsync(__request, __cancellationToken);
        return PaginationResponse<SeasonResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<SeasonResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var season = await _seasonRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Season", __id);
        return ToResponse(season);
    }

    public async Task<SeasonResponse> CreateAsync(SeasonRequest __request, CancellationToken __cancellationToken)
    {
        var existing = await _seasonRepository.GetByNameAsync(__request.Name, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A season named '{__request.Name}' already exists.");
        }

        var season = new SeasonModel
        {
            Id = Guid.NewGuid(),
            Name = __request.Name,
            StartMonth = __request.StartMonth,
            EndMonth = __request.EndMonth,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _seasonRepository.CreateAsync(season, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "season.created", "Season", season.Id.ToString(), null, season.Name, __cancellationToken);

        return ToResponse(season);
    }

    public async Task<SeasonResponse> UpdateAsync(Guid __id, SeasonRequest __request, CancellationToken __cancellationToken)
    {
        var season = await _seasonRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Season", __id);

        var oldName = season.Name;
        season.Name = __request.Name;
        season.StartMonth = __request.StartMonth;
        season.EndMonth = __request.EndMonth;
        season.UpdatedBy = _currentUserAccessor.UserId;

        await _seasonRepository.UpdateAsync(season, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "season.updated", "Season", season.Id.ToString(), oldName, season.Name, __cancellationToken);

        return ToResponse(season);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var season = await _seasonRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Season", __id);

        await _seasonRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "season.deleted", "Season", __id.ToString(), season.Name, null, __cancellationToken);
    }

    private static SeasonResponse ToResponse(SeasonModel season) => new()
    {
        Id = season.Id,
        Name = season.Name,
        StartMonth = season.StartMonth,
        EndMonth = season.EndMonth
    };
}
