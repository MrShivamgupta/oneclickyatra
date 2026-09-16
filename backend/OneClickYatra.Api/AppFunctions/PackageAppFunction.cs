using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class PackageAppFunction : IPackageAppFunction
{
    private readonly IPackageRepository _packageRepository;
    private readonly IPackageContentRepository _packageContentRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public PackageAppFunction(
        IPackageRepository __packageRepository,
        IPackageContentRepository __packageContentRepository,
        IDestinationRepository __destinationRepository,
        ICategoryRepository __categoryRepository,
        ISeasonRepository __seasonRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter)
    {
        _packageRepository = __packageRepository;
        _packageContentRepository = __packageContentRepository;
        _destinationRepository = __destinationRepository;
        _categoryRepository = __categoryRepository;
        _seasonRepository = __seasonRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    public async Task<PaginationResponse<PackageResponse>> SearchAsync(PackageSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _packageRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<PackageResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<PackageDetailResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Package", __id);

        var itinerary = await _packageContentRepository.GetItineraryAsync(__id, __cancellationToken);
        var inclusions = await _packageContentRepository.GetInclusionsAsync(__id, __cancellationToken);
        var pricing = await _packageContentRepository.GetPricingAsync(__id, __cancellationToken);
        var inventory = await _packageContentRepository.GetInventoryAsync(__id, __cancellationToken);
        var media = await _packageContentRepository.GetMediaAsync(__id, __cancellationToken);

        return new PackageDetailResponse
        {
            Package = ToResponse(package),
            Itinerary = itinerary.Select(ToResponse).ToList(),
            Inclusions = inclusions.Select(ToResponse).ToList(),
            Pricing = pricing.Select(ToResponse).ToList(),
            Inventory = inventory.Select(ToResponse).ToList(),
            Media = media.Select(ToResponse).ToList()
        };
    }

    /// <summary>Public: used for pretty public-site URLs like /packages/goa-family-getaway. Never exposes non-Published packages.</summary>
    public async Task<PackageDetailResponse> GetBySlugAsync(string __slug, CancellationToken __cancellationToken)
    {
        var package = await _packageRepository.GetBySlugAsync(__slug, __cancellationToken);
        if (package is null || package.Status != "Published")
        {
            throw new EntityNotFoundException("Package", __slug);
        }

        var itinerary = await _packageContentRepository.GetItineraryAsync(package.Id, __cancellationToken);
        var inclusions = await _packageContentRepository.GetInclusionsAsync(package.Id, __cancellationToken);
        var pricing = await _packageContentRepository.GetPricingAsync(package.Id, __cancellationToken);
        var inventory = await _packageContentRepository.GetInventoryAsync(package.Id, __cancellationToken);
        var media = await _packageContentRepository.GetMediaAsync(package.Id, __cancellationToken);

        return new PackageDetailResponse
        {
            Package = ToResponse(package),
            Itinerary = itinerary.Select(ToResponse).ToList(),
            Inclusions = inclusions.Select(ToResponse).ToList(),
            Pricing = pricing.Select(ToResponse).ToList(),
            Inventory = inventory.Select(ToResponse).ToList(),
            Media = media.Select(ToResponse).ToList()
        };
    }

    public async Task<PackageResponse> CreateAsync(PackageRequest __request, CancellationToken __cancellationToken)
    {
        await ValidateReferencesAsync(__request, __cancellationToken);

        var existing = await _packageRepository.GetBySlugAsync(__request.Slug, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A package with slug '{__request.Slug}' already exists.");
        }

        var package = new PackageModel
        {
            Id = Guid.NewGuid(),
            DestinationId = __request.DestinationId,
            CategoryId = __request.CategoryId,
            SeasonId = __request.SeasonId,
            Title = __request.Title,
            Slug = __request.Slug,
            DurationDays = __request.DurationDays,
            DurationNights = __request.DurationNights,
            ShortDescription = __request.ShortDescription,
            Description = __request.Description,
            HeroImageUrl = __request.HeroImageUrl,
            Status = "Draft",
            CreatedBy = _currentUserAccessor.UserId
        };

        await _packageRepository.CreateAsync(package, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.created", "Package", package.Id.ToString(), null, package.Title, __cancellationToken);

        var created = await _packageRepository.GetByIdAsync(package.Id, __cancellationToken) ?? package;
        return ToResponse(created);
    }

    public async Task<PackageResponse> UpdateAsync(Guid __id, PackageRequest __request, CancellationToken __cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Package", __id);
        await ValidateReferencesAsync(__request, __cancellationToken);

        var oldTitle = package.Title;
        package.DestinationId = __request.DestinationId;
        package.CategoryId = __request.CategoryId;
        package.SeasonId = __request.SeasonId;
        package.Title = __request.Title;
        package.Slug = __request.Slug;
        package.DurationDays = __request.DurationDays;
        package.DurationNights = __request.DurationNights;
        package.ShortDescription = __request.ShortDescription;
        package.Description = __request.Description;
        package.HeroImageUrl = __request.HeroImageUrl;
        package.UpdatedBy = _currentUserAccessor.UserId;

        await _packageRepository.UpdateAsync(package, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.updated", "Package", package.Id.ToString(), oldTitle, package.Title, __cancellationToken);

        var updated = await _packageRepository.GetByIdAsync(__id, __cancellationToken) ?? package;
        return ToResponse(updated);
    }

    public async Task<PackageResponse> UpdateStatusAsync(Guid __id, PackageStatusRequest __request, CancellationToken __cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Package", __id);

        var oldStatus = package.Status;
        await _packageRepository.UpdateStatusAsync(__id, __request.Status, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.status.changed", "Package", __id.ToString(), oldStatus, __request.Status, __cancellationToken);

        var updated = await _packageRepository.GetByIdAsync(__id, __cancellationToken) ?? package;
        return ToResponse(updated);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Package", __id);

        await _packageRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.deleted", "Package", __id.ToString(), package.Title, null, __cancellationToken);
    }

    public async Task<IReadOnlyList<PackageItineraryDayResponse>> ReplaceItineraryAsync(Guid __packageId, IReadOnlyList<PackageItineraryDayRequest> __days, CancellationToken __cancellationToken)
    {
        await EnsurePackageExistsAsync(__packageId, __cancellationToken);
        EnsureNoDuplicateDayNumbers(__days);

        var models = __days.Select(day => new PackageItineraryDayModel
        {
            PackageId = __packageId,
            DayNumber = day.DayNumber,
            Title = day.Title,
            Description = day.Description
        }).ToList();

        await _packageContentRepository.ReplaceItineraryAsync(__packageId, models, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.itinerary.replaced", "Package", __packageId.ToString(), null, $"{models.Count} day(s)", __cancellationToken);

        var saved = await _packageContentRepository.GetItineraryAsync(__packageId, __cancellationToken);
        return saved.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<PackageInclusionResponse>> ReplaceInclusionsAsync(Guid __packageId, IReadOnlyList<PackageInclusionRequest> __inclusions, CancellationToken __cancellationToken)
    {
        await EnsurePackageExistsAsync(__packageId, __cancellationToken);

        var models = __inclusions.Select(inclusion => new PackageInclusionModel
        {
            PackageId = __packageId,
            Description = inclusion.Description,
            IsIncluded = inclusion.IsIncluded,
            SortOrder = inclusion.SortOrder
        }).ToList();

        await _packageContentRepository.ReplaceInclusionsAsync(__packageId, models, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.inclusions.replaced", "Package", __packageId.ToString(), null, $"{models.Count} item(s)", __cancellationToken);

        var saved = await _packageContentRepository.GetInclusionsAsync(__packageId, __cancellationToken);
        return saved.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<PackagePricingTierResponse>> ReplacePricingAsync(Guid __packageId, IReadOnlyList<PackagePricingTierRequest> __tiers, CancellationToken __cancellationToken)
    {
        await EnsurePackageExistsAsync(__packageId, __cancellationToken);

        var models = __tiers.Select(tier => new PackagePricingTierModel
        {
            PackageId = __packageId,
            TierName = tier.TierName,
            HotelCategory = tier.HotelCategory,
            PricePerPerson = tier.PricePerPerson,
            ChildPrice = tier.ChildPrice,
            ValidFrom = tier.ValidFrom,
            ValidTo = tier.ValidTo,
            Currency = tier.Currency
        }).ToList();

        await _packageContentRepository.ReplacePricingAsync(__packageId, models, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.pricing.replaced", "Package", __packageId.ToString(), null, $"{models.Count} tier(s)", __cancellationToken);

        var saved = await _packageContentRepository.GetPricingAsync(__packageId, __cancellationToken);
        return saved.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<PackageInventoryResponse>> ReplaceInventoryAsync(Guid __packageId, IReadOnlyList<PackageInventoryRequest> __departures, CancellationToken __cancellationToken)
    {
        await EnsurePackageExistsAsync(__packageId, __cancellationToken);
        EnsureNoDuplicateDepartureDates(__departures);

        var models = __departures.Select(departure => new PackageInventoryModel
        {
            PackageId = __packageId,
            DepartureDate = departure.DepartureDate,
            TotalSeats = departure.TotalSeats,
            BookedSeats = departure.BookedSeats,
            Status = departure.Status
        }).ToList();

        await _packageContentRepository.ReplaceInventoryAsync(__packageId, models, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.inventory.replaced", "Package", __packageId.ToString(), null, $"{models.Count} departure(s)", __cancellationToken);

        var saved = await _packageContentRepository.GetInventoryAsync(__packageId, __cancellationToken);
        return saved.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<PackageMediaResponse>> ReplaceMediaAsync(Guid __packageId, IReadOnlyList<PackageMediaRequest> __media, CancellationToken __cancellationToken)
    {
        await EnsurePackageExistsAsync(__packageId, __cancellationToken);

        var models = __media.Select(media => new PackageMediaModel
        {
            PackageId = __packageId,
            MediaUrl = media.MediaUrl,
            MediaType = media.MediaType,
            SortOrder = media.SortOrder,
            IsCoverImage = media.IsCoverImage
        }).ToList();

        await _packageContentRepository.ReplaceMediaAsync(__packageId, models, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "package.media.replaced", "Package", __packageId.ToString(), null, $"{models.Count} item(s)", __cancellationToken);

        var saved = await _packageContentRepository.GetMediaAsync(__packageId, __cancellationToken);
        return saved.Select(ToResponse).ToList();
    }

    private async Task EnsurePackageExistsAsync(Guid __packageId, CancellationToken __cancellationToken)
    {
        _ = await _packageRepository.GetByIdAsync(__packageId, __cancellationToken)
            ?? throw new EntityNotFoundException("Package", __packageId);
    }

    private async Task ValidateReferencesAsync(PackageRequest __request, CancellationToken __cancellationToken)
    {
        _ = await _destinationRepository.GetByIdAsync(__request.DestinationId, __cancellationToken)
            ?? throw new EntityNotFoundException("Destination", __request.DestinationId);

        if (__request.CategoryId is { } categoryId)
        {
            _ = await _categoryRepository.GetByIdAsync(categoryId, __cancellationToken)
                ?? throw new EntityNotFoundException("Category", categoryId);
        }

        if (__request.SeasonId is { } seasonId)
        {
            _ = await _seasonRepository.GetByIdAsync(seasonId, __cancellationToken)
                ?? throw new EntityNotFoundException("Season", seasonId);
        }
    }

    private static void EnsureNoDuplicateDayNumbers(IReadOnlyList<PackageItineraryDayRequest> __days)
    {
        var duplicate = __days.GroupBy(day => day.DayNumber).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new BusinessException($"Day {duplicate.Key} appears more than once in the itinerary.");
        }
    }

    private static void EnsureNoDuplicateDepartureDates(IReadOnlyList<PackageInventoryRequest> __departures)
    {
        var duplicate = __departures.GroupBy(departure => departure.DepartureDate).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new BusinessException($"Departure date {duplicate.Key:yyyy-MM-dd} appears more than once.");
        }
    }

    private static PackageResponse ToResponse(PackageModel package) => new()
    {
        Id = package.Id,
        DestinationId = package.DestinationId,
        DestinationName = package.DestinationName ?? string.Empty,
        CategoryId = package.CategoryId,
        CategoryName = package.CategoryName,
        SeasonId = package.SeasonId,
        SeasonName = package.SeasonName,
        Title = package.Title,
        Slug = package.Slug,
        DurationDays = package.DurationDays,
        DurationNights = package.DurationNights,
        ShortDescription = package.ShortDescription,
        Description = package.Description,
        HeroImageUrl = package.HeroImageUrl,
        Status = package.Status,
        StartingPricePerPerson = package.StartingPricePerPerson,
        PriceCurrency = package.PriceCurrency
    };

    private static PackageItineraryDayResponse ToResponse(PackageItineraryDayModel day) => new()
    {
        Id = day.Id,
        DayNumber = day.DayNumber,
        Title = day.Title,
        Description = day.Description
    };

    private static PackageInclusionResponse ToResponse(PackageInclusionModel inclusion) => new()
    {
        Id = inclusion.Id,
        Description = inclusion.Description,
        IsIncluded = inclusion.IsIncluded,
        SortOrder = inclusion.SortOrder
    };

    private static PackagePricingTierResponse ToResponse(PackagePricingTierModel tier) => new()
    {
        Id = tier.Id,
        TierName = tier.TierName,
        HotelCategory = tier.HotelCategory,
        PricePerPerson = tier.PricePerPerson,
        ChildPrice = tier.ChildPrice,
        ValidFrom = tier.ValidFrom,
        ValidTo = tier.ValidTo,
        Currency = tier.Currency
    };

    private static PackageInventoryResponse ToResponse(PackageInventoryModel inventory) => new()
    {
        Id = inventory.Id,
        DepartureDate = inventory.DepartureDate,
        TotalSeats = inventory.TotalSeats,
        BookedSeats = inventory.BookedSeats,
        AvailableSeats = inventory.AvailableSeats,
        Status = inventory.Status
    };

    private static PackageMediaResponse ToResponse(PackageMediaModel media) => new()
    {
        Id = media.Id,
        MediaUrl = media.MediaUrl,
        MediaType = media.MediaType,
        SortOrder = media.SortOrder,
        IsCoverImage = media.IsCoverImage
    };
}
