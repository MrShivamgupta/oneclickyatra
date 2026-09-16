namespace OneClickYatra.Api.Models;

public sealed class PackageMediaModel
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Image";
    public int SortOrder { get; set; }
    public bool IsCoverImage { get; set; }
}
