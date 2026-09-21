namespace OneClickYatra.Api.Services.Storage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Absolute or content-root-relative path outside wwwroot. Empty means "use the
    /// default" (App_Data/uploads under the content root) — uploaded files are never served as
    /// static content, only ever streamed back through an authenticated, ownership-checked
    /// download endpoint, so there is no need for this to sit under wwwroot at all.</summary>
    public string LocalRootPath { get; set; } = string.Empty;
}
