using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class PackageModel : IPackageModel
{
    public Guid Id { get; set; }
    public Guid DestinationId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SeasonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int DurationNights { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Populated by JOINs in read queries; not physical columns.
    public string? DestinationName { get; set; }
    public string? CategoryName { get; set; }
    public string? SeasonName { get; set; }

    // Populated by pricing subquery in list/search reads.
    public decimal? StartingPricePerPerson { get; set; }
    public string? PriceCurrency { get; set; }
}
