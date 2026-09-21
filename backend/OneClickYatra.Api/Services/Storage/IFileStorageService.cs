namespace OneClickYatra.Api.Services.Storage;

public sealed record StoredFile(Stream Content, string ContentType, string FileName);

/// <summary>Generic file storage abstraction — not booking/vendor-specific, so any future domain
/// can reuse it. The local disk implementation is swappable for S3/Azure Blob later without
/// touching any caller; callers only ever see an opaque storage path/key, never a public URL,
/// since every upload is served back through an authenticated, ownership-checked endpoint rather
/// than a static file path.</summary>
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream __content, string __fileName, string __contentType, CancellationToken __cancellationToken);
    Task<StoredFile> ReadAsync(string __storagePath, CancellationToken __cancellationToken);
    Task DeleteAsync(string __storagePath, CancellationToken __cancellationToken);
}
