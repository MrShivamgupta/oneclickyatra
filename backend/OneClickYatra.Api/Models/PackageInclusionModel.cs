namespace OneClickYatra.Api.Models;

public sealed class PackageInclusionModel
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsIncluded { get; set; } = true;
    public int SortOrder { get; set; }
}
