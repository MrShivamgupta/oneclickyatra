using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.UnitTests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), "oneclickyatra-tests", Guid.NewGuid().ToString("N"));

    private LocalFileStorageService CreateSut()
    {
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(e => e.ContentRootPath).Returns(_rootPath);
        return new LocalFileStorageService(
            Options.Create(new FileStorageOptions()),
            hostEnvironment.Object,
            Mock.Of<ILogger<LocalFileStorageService>>());
    }

    [Fact]
    public async Task SaveAsync_DisallowedContentType_ThrowsBusinessException()
    {
        var sut = CreateSut();
        using var content = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<BusinessException>(() => sut.SaveAsync(content, "malware.exe", "application/x-msdownload", CancellationToken.None));
    }

    [Fact]
    public async Task SaveAsync_ExceedsMaxSize_ThrowsBusinessException()
    {
        var sut = CreateSut();
        using var content = new MemoryStream(new byte[11 * 1024 * 1024]);

        await Assert.ThrowsAsync<BusinessException>(() => sut.SaveAsync(content, "big.pdf", "application/pdf", CancellationToken.None));
    }

    [Fact]
    public async Task SaveAsync_ThenReadAsync_RoundTripsTheContent()
    {
        var sut = CreateSut();
        var bytes = new byte[] { 10, 20, 30, 40 };
        using var content = new MemoryStream(bytes);

        var storagePath = await sut.SaveAsync(content, "receipt.pdf", "application/pdf", CancellationToken.None);
        var stored = await sut.ReadAsync(storagePath, CancellationToken.None);

        using var reader = new MemoryStream();
        await stored.Content.CopyToAsync(reader);
        Assert.Equal(bytes, reader.ToArray());
        Assert.Equal("application/pdf", stored.ContentType);
    }

    [Fact]
    public async Task SaveAsync_FileNameWithPathTraversal_ThrowsBusinessException()
    {
        var sut = CreateSut();
        using var content = new MemoryStream([1]);

        await Assert.ThrowsAsync<BusinessException>(() => sut.SaveAsync(content, "../../etc/passwd", "application/pdf", CancellationToken.None));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }
}
