using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
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

    private PackageAppFunction CreateSut() => new(
        _packageRepository.Object,
        _packageContentRepository.Object,
        _destinationRepository.Object,
        _categoryRepository.Object,
        _seasonRepository.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object);

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
    }
}
