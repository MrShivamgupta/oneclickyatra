using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class PackageAppFunctionTests
{
    private readonly Mock<IPackageRepository> _packageRepository = new();
    private readonly Mock<IPackageContentRepository> _packageContentRepository = new();
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<ISeasonRepository> _seasonRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();
    private readonly Mock<ICacheService> _cacheService = new();

    private PackageAppFunction CreateSut()
    {
        _cacheService
            .Setup(c => c.GetAsync<PackageDetailResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageDetailResponse?)null);

        return new PackageAppFunction(
            _packageRepository.Object,
            _packageContentRepository.Object,
            _destinationRepository.Object,
            _categoryRepository.Object,
            _seasonRepository.Object,
            _currentUserAccessor.Object,
            _auditLogWriter.Object,
            _cacheService.Object,
            Mock.Of<ILogger<PackageAppFunction>>());
    }

    private static PackageModel CreatePackage(string slug = "goa-family-getaway", string status = "Published") => new()
    {
        Id = Guid.NewGuid(),
        DestinationId = Guid.NewGuid(),
        Title = "Goa Family Getaway",
        Slug = slug,
        DurationDays = 4,
        DurationNights = 3,
        Status = status,
        DestinationName = "Goa"
    };

    [Fact]
    public async Task GetBySlugAsync_UnknownSlug_ThrowsEntityNotFound()
    {
        _packageRepository.Setup(r => r.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PackageModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.GetBySlugAsync("missing-slug", CancellationToken.None));
    }

    [Fact]
    public async Task GetBySlugAsync_DraftPackage_ThrowsEntityNotFound()
    {
        var package = CreatePackage(status: "Draft");
        _packageRepository.Setup(r => r.GetBySlugAsync(package.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.GetBySlugAsync(package.Slug, CancellationToken.None));

        _packageContentRepository.Verify(r => r.GetItineraryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBySlugAsync_PublishedPackage_ReturnsFullDetail()
    {
        var package = CreatePackage();
        _packageRepository.Setup(r => r.GetBySlugAsync(package.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);
        _packageContentRepository.Setup(r => r.GetItineraryAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PackageItineraryDayModel { Id = Guid.NewGuid(), PackageId = package.Id, DayNumber = 1, Title = "Arrival" }]);
        _packageContentRepository.Setup(r => r.GetInclusionsAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _packageContentRepository.Setup(r => r.GetPricingAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _packageContentRepository.Setup(r => r.GetInventoryAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _packageContentRepository.Setup(r => r.GetMediaAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CreateSut();
        var result = await sut.GetBySlugAsync(package.Slug, CancellationToken.None);

        Assert.Equal(package.Id, result.Package.Id);
        Assert.Equal(package.Slug, result.Package.Slug);
        Assert.Single(result.Itinerary);
        Assert.Equal("Arrival", result.Itinerary[0].Title);
        _cacheService.Verify(c => c.SetAsync(
            It.Is<string>(key => key.Contains(package.Slug)),
            It.IsAny<PackageDetailResponse>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySlugAsync_CacheHit_ReturnsCachedResponseWithoutHittingRepository()
    {
        var cached = new PackageDetailResponse
        {
            Package = new PackageResponse { Id = Guid.NewGuid(), Slug = "goa-family-getaway", Title = "Goa Family Getaway", DestinationName = "Goa", Status = "Published" }
        };

        // CreateSut() configures a default (cache-miss) setup for this same mocked method, so the
        // cache-hit override must be applied AFTER creating the sut, not before — otherwise
        // CreateSut()'s default setup clobbers this test's intent and it falls through to the
        // repository, which isn't set up here and returns null.
        var sut = CreateSut();
        _cacheService
            .Setup(c => c.GetAsync<PackageDetailResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await sut.GetBySlugAsync("goa-family-getaway", CancellationToken.None);

        Assert.Same(cached, result);
        _packageRepository.Verify(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedResponseWithoutHittingRepository()
    {
        var cached = new PackageDetailResponse
        {
            Package = new PackageResponse { Id = Guid.NewGuid(), Slug = "goa-family-getaway", Title = "Goa Family Getaway", DestinationName = "Goa", Status = "Published" }
        };

        var sut = CreateSut();
        _cacheService
            .Setup(c => c.GetAsync<PackageDetailResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await sut.GetByIdAsync(cached.Package.Id, CancellationToken.None);

        Assert.Same(cached, result);
        _packageRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCacheByPackagePrefix()
    {
        var package = CreatePackage();
        _packageRepository.Setup(r => r.GetByIdAsync(package.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        var sut = CreateSut();
        await sut.DeleteAsync(package.Id, CancellationToken.None);

        _cacheService.Verify(c => c.RemoveByPrefixAsync("package:", It.IsAny<CancellationToken>()), Times.Once);
    }
}
