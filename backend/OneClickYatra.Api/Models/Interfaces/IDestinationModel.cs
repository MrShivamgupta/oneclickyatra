namespace OneClickYatra.Api.Models.Interfaces;

public interface IDestinationModel
{
    Guid Id { get; }
    Guid CountryId { get; }
    Guid? CityId { get; }
    string Name { get; }
    string Slug { get; }
    string? ShortDescription { get; }
    string? Description { get; }
    string? HeroImageUrl { get; }
    bool IsFeatured { get; }
    bool IsPublished { get; }
    bool IsDeleted { get; }
}
