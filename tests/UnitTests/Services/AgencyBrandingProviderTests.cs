using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.Services;

public class AgencyBrandingProviderTests
{
    private readonly Mock<IAgencyProfileRepository> _agencyProfileRepository = new();

    private static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return new HttpClient(handler.Object);
    }

    private AgencyBrandingProvider CreateSut(HttpClient __httpClient) =>
        new(__httpClient, _agencyProfileRepository.Object, NullLogger<AgencyBrandingProvider>.Instance);

    [Fact]
    public async Task GetProfileAsync_RowExists_ReturnsIt()
    {
        var profile = new AgencyProfileModel { Name = "Real Agency", Currency = "USD" };
        _agencyProfileRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var sut = CreateSut(CreateHttpClient(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        var result = await sut.GetProfileAsync(CancellationToken.None);

        Assert.Equal("Real Agency", result.Name);
    }

    [Fact]
    public async Task GetProfileAsync_RowMissing_FallsBackToDefault_NeverThrows()
    {
        _agencyProfileRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((AgencyProfileModel?)null);

        var sut = CreateSut(CreateHttpClient(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        var result = await sut.GetProfileAsync(CancellationToken.None);

        Assert.Equal("One Click Yatra", result.Name);
    }

    [Fact]
    public async Task GetProfileAsync_RepositoryThrows_FallsBackToDefault_NeverThrows()
    {
        _agencyProfileRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("db down"));

        var sut = CreateSut(CreateHttpClient(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
        var result = await sut.GetProfileAsync(CancellationToken.None);

        Assert.Equal("One Click Yatra", result.Name);
    }

    [Fact]
    public async Task TryFetchLogoBytesAsync_NullOrEmptyUrl_ReturnsNullWithoutCallingHttp()
    {
        var sut = CreateSut(CreateHttpClient(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

        Assert.Null(await sut.TryFetchLogoBytesAsync(null, CancellationToken.None));
        Assert.Null(await sut.TryFetchLogoBytesAsync("", CancellationToken.None));
        Assert.Null(await sut.TryFetchLogoBytesAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task TryFetchLogoBytesAsync_ValidPngSignature_ReturnsBytes()
    {
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02 };
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(pngBytes) };

        var sut = CreateSut(CreateHttpClient(response));
        var result = await sut.TryFetchLogoBytesAsync("https://example.com/logo.png", CancellationToken.None);

        Assert.Equal(pngBytes, result);
    }

    [Fact]
    public async Task TryFetchLogoBytesAsync_ResponseIsNotAnImage_ReturnsNull()
    {
        var htmlErrorPage = System.Text.Encoding.UTF8.GetBytes("<html>404 not found</html>");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(htmlErrorPage) };

        var sut = CreateSut(CreateHttpClient(response));
        var result = await sut.TryFetchLogoBytesAsync("https://example.com/broken-logo-url", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TryFetchLogoBytesAsync_HttpCallThrows_ReturnsNullWithoutThrowing()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network error"));

        var sut = CreateSut(new HttpClient(handler.Object));
        var result = await sut.TryFetchLogoBytesAsync("https://unreachable.example.com/logo.png", CancellationToken.None);

        Assert.Null(result);
    }
}
