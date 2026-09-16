using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class PageAppFunctionTests
{
    private readonly Mock<IPageRepository> _pageRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private PageAppFunction CreateSut() => new(_pageRepository.Object, _currentUserAccessor.Object, _auditLogWriter.Object);

    private static PageModel CreatePage(string slug = "about", bool isPublished = true) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        Title = "About Us",
        Content = "Some content.",
        IsPublished = isPublished
    };

    [Fact]
    public async Task GetPublishedBySlugAsync_UnpublishedPage_ThrowsEntityNotFound()
    {
        var page = CreatePage(isPublished: false);
        _pageRepository.Setup(r => r.GetBySlugAsync(page.Slug, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.GetPublishedBySlugAsync(page.Slug, CancellationToken.None));
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_MissingSlug_ThrowsEntityNotFound()
    {
        _pageRepository.Setup(r => r.GetBySlugAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((PageModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.GetPublishedBySlugAsync("missing", CancellationToken.None));
    }

    [Fact]
    public async Task GetPublishedBySlugAsync_PublishedPage_ReturnsResponse()
    {
        var page = CreatePage();
        _pageRepository.Setup(r => r.GetBySlugAsync(page.Slug, It.IsAny<CancellationToken>())).ReturnsAsync(page);

        var sut = CreateSut();
        var result = await sut.GetPublishedBySlugAsync(page.Slug, CancellationToken.None);

        Assert.Equal(page.Title, result.Title);
        Assert.Equal(page.Slug, result.Slug);
    }

    [Fact]
    public async Task CreateAsync_SlugAlreadyExists_ThrowsBusinessException()
    {
        var existing = CreatePage();
        _pageRepository.Setup(r => r.GetBySlugAsync(existing.Slug, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var sut = CreateSut();
        var request = new PageRequest { Slug = existing.Slug, Title = "Duplicate", IsPublished = true };

        await Assert.ThrowsAsync<BusinessException>(() => sut.CreateAsync(request, CancellationToken.None));
        _pageRepository.Verify(r => r.CreateAsync(It.IsAny<PageModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SlugTakenByAnotherPage_ThrowsBusinessException()
    {
        var page = CreatePage(slug: "contact");
        var other = CreatePage(slug: "terms");
        _pageRepository.Setup(r => r.GetByIdAsync(page.Id, It.IsAny<CancellationToken>())).ReturnsAsync(page);
        _pageRepository.Setup(r => r.GetBySlugAsync(other.Slug, It.IsAny<CancellationToken>())).ReturnsAsync(other);

        var sut = CreateSut();
        var request = new PageRequest { Slug = other.Slug, Title = "Contact Us", IsPublished = true };

        await Assert.ThrowsAsync<BusinessException>(() => sut.UpdateAsync(page.Id, request, CancellationToken.None));
        _pageRepository.Verify(r => r.UpdateAsync(It.IsAny<PageModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_MissingPage_ThrowsEntityNotFound()
    {
        var id = Guid.NewGuid();
        _pageRepository.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PageModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(id, CancellationToken.None));
        _pageRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
