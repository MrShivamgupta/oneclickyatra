using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class CountryRequest
{
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
}

public sealed class CityRequest
{
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class SeasonRequest
{
    public string Name { get; set; } = string.Empty;
    public int StartMonth { get; set; }
    public int EndMonth { get; set; }
}

public sealed class DestinationRequest
{
    public Guid CountryId { get; set; }
    public Guid? CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
}

public sealed class DestinationSearchRequest : PaginationRequest
{
    public Guid? CountryId { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsPublished { get; set; }
}
