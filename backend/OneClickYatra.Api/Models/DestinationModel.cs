using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class DestinationModel : IDestinationModel
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public Guid? CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Populated by JOINs in read queries; not physical columns.
    public string? CountryName { get; set; }
    public string? CityName { get; set; }
}
