namespace OneClickYatra.Api.Models.Responses;

public sealed class CountryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
}

public sealed class CityResponse
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class SeasonResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartMonth { get; set; }
    public int EndMonth { get; set; }
}

public sealed class DestinationResponse
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public Guid? CityId { get; set; }
    public string? CityName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
}
