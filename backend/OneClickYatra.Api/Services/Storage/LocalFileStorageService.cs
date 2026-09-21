using Microsoft.Extensions.Options;
using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Services.Storage;

/// <summary>
/// Saves files to a local directory outside wwwroot. Enforces the size/content-type limits itself
/// (not left to callers to remember) so every caller of IFileStorageService gets the same
/// protection for free. Swappable for an S3/Azure Blob implementation later without any caller
/// change, since IFileStorageService only ever deals in opaque storage paths.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<FileStorageOptions> __options, IHostEnvironment __hostEnvironment, ILogger<LocalFileStorageService> __logger)
    {
        var configuredRoot = __options.Value.LocalRootPath;
        _rootPath = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(__hostEnvironment.ContentRootPath, "App_Data", "uploads")
            : configuredRoot;
        _logger = __logger;

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream __content, string __fileName, string __contentType, CancellationToken __cancellationToken)
    {
        if (!AllowedContentTypes.Contains(__contentType))
        {
            throw new BusinessException($"File type '{__contentType}' is not allowed. Allowed types: PDF, JPEG, PNG, WEBP, DOCX.");
        }

        var safeFileName = SanitizeFileName(__fileName);
        var storagePath = Path.Combine(Guid.NewGuid().ToString("N"), safeFileName);
        var fullPath = Path.Combine(_rootPath, storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        await CopyWithSizeLimitAsync(__content, destination, __cancellationToken);

        _logger.LogInformation("Stored file {StoragePath} ({ContentType}, {SizeBytes} bytes)", storagePath, __contentType, destination.Length);
        return storagePath.Replace('\\', '/');
    }

    public async Task<StoredFile> ReadAsync(string __storagePath, CancellationToken __cancellationToken)
    {
        var fullPath = ResolveAndValidatePath(__storagePath);
        if (!File.Exists(fullPath))
        {
            throw new EntityNotFoundException("File", __storagePath);
        }

        var content = new MemoryStream();
        await using (var source = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
        {
            await source.CopyToAsync(content, __cancellationToken);
        }
        content.Position = 0;

        var fileName = Path.GetFileName(fullPath);
        return new StoredFile(content, ContentTypeFor(fileName), fileName);
    }

    public Task DeleteAsync(string __storagePath, CancellationToken __cancellationToken)
    {
        var fullPath = ResolveAndValidatePath(__storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    /// <summary>Copies in chunks, throwing as soon as the running total exceeds the limit, rather
    /// than buffering the whole stream first — a caller cannot exhaust memory/disk by lying about
    /// Content-Length (ASP.NET Core's own request body size limit is a separate, coarser guard).</summary>
    private static async Task CopyWithSizeLimitAsync(Stream __source, Stream __destination, CancellationToken __cancellationToken)
    {
        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await __source.ReadAsync(buffer, __cancellationToken)) > 0)
        {
            total += read;
            if (total > MaxFileSizeBytes)
            {
                throw new BusinessException($"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");
            }
            await __destination.WriteAsync(buffer.AsMemory(0, read), __cancellationToken);
        }
    }

    /// <summary>Strips any directory component and disallows "..", keeping only the file's base
    /// name and extension — the storage path itself already carries a fresh Guid segment, so this
    /// only needs to stop the original filename from escaping that segment's directory.</summary>
    private static string SanitizeFileName(string __fileName)
    {
        if (__fileName.Contains(".."))
        {
            throw new BusinessException("Invalid file name.");
        }

        var name = Path.GetFileName(__fileName);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessException("Invalid file name.");
        }
        return name;
    }

    /// <summary>Re-validates a storage path read back from the database before touching the file
    /// system with it — defense in depth in case a path was ever malformed at write time.</summary>
    private string ResolveAndValidatePath(string __storagePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, __storagePath));
        var rootFullPath = Path.GetFullPath(_rootPath);
        if (!fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Invalid storage path.");
        }
        return fullPath;
    }

    private static string ContentTypeFor(string __fileName) => Path.GetExtension(__fileName).ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };
}
