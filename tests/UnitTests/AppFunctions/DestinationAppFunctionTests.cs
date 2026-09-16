using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class DestinationAppFunctionTests
{
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<ICountryRepository> _countryRepository = new();
    private readonly Mock<ICityRepository> _cityRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();
    private readonly Mock<ICacheService> _cacheService = new();

    private DestinationAppFunction CreateSut()
    {
        _cacheService
            .Setup(c => c.GetAsync<DestinationResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DestinationResponse?)null);

        return new DestinationAppFunction(
            _destinationRepository.Object,
            _countryRepository.Object,
            _cityRepository.Object,
            _currentUserAccessor.Object,
            _auditLogWriter.Object,
            _cacheService.Object);
    }

    private static DestinationModel CreatePublishedDestination(string slug = "goa-india") => new()
    {
        Id = Guid.NewGuid(),
        CountryId = Guid.NewGuid(),
        Name = "Goa",
        Slug = slug,
        IsPublished = true,
        CountryName = "India"
    };

    [Fact]
    public async Task GetBySlugAsync_UnknownSlug_ThrowsEntityNotFound()
    {
        _destinationRepository.Setup(r => r.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DestinationModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.GetBySlugAsync("missing-slug", CancellationToken.None));
    }

    [Fact]
    public async Task GetBySlugAsync_UnpublishedDestination_ThrowsEntityNotFound()
    {
        var destination = CreatePublishedDestination();
        destination.IsPublished = false;
        _destinationRepository.Setup(r => r.GetBySlugAsync(destination.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.GetBySlugAsync(destination.Slug, CancellationToken.None));
    }

    [Fact]
    public async Task GetBySlugAsync_PublishedDestination_ReturnsResponseAndCachesIt()
    {
        var destination = CreatePublishedDestination();
        _destinationRepository.Setup(r => r.GetBySlugAsync(destination.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destination);

        var sut = CreateSut();
        var result = await sut.GetBySlugAsync(destination.Slug, CancellationToken.None);

        Assert.Equal(destination.Id, result.Id);
        Assert.Equal(destination.Slug, result.Slug);
        _cacheService.Verify(c => c.SetAsync(
            It.Is<string>(key => key.Contains(destination.Slug)),
            It.IsAny<DestinationResponse>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySlugAsync_CacheHit_ReturnsCachedResponseWithoutHittingRepository()
    {
        var cached = new DestinationResponse { Id = Guid.NewGuid(), Slug = "goa-india", Name = "Goa", CountryName = "India" };

        // CreateSut() configures a default (cache-miss) setup for this same mocked method, so the
        // cache-hit override must be applied AFTER creating the sut, not before — otherwise
        // CreateSut()'s default setup clobbers this test's intent and it falls through to the
        // repository, which isn't set up here and returns null.
        var sut = CreateSut();
        _cacheService
            .Setup(c => c.GetAsync<DestinationResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await sut.GetBySlugAsync("goa-india", CancellationToken.None);

        Assert.Same(cached, result);
        _destinationRepository.Verify(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
