namespace OneClickYatra.Api.Models.Responses;

public sealed class PackageResponse
{
    public Guid Id { get; set; }
    public Guid DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? SeasonId { get; set; }
    public string? SeasonName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int DurationNights { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? HeroImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? StartingPricePerPerson { get; set; }
    public string? PriceCurrency { get; set; }
}

public sealed class PackageItineraryDayResponse
{
    public Guid Id { get; set; }
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class PackageInclusionResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsIncluded { get; set; }
    public int SortOrder { get; set; }
}

public sealed class PackagePricingTierResponse
{
    public Guid Id { get; set; }
    public string TierName { get; set; } = string.Empty;
    public string? HotelCategory { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal? ChildPrice { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public sealed class PackageInventoryResponse
{
    public Guid Id { get; set; }
    public DateOnly DepartureDate { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class PackageMediaResponse
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCoverImage { get; set; }
}

public sealed class PackageDetailResponse
{
    public PackageResponse Package { get; set; } = new();
    public IReadOnlyList<PackageItineraryDayResponse> Itinerary { get; set; } = Array.Empty<PackageItineraryDayResponse>();
    public IReadOnlyList<PackageInclusionResponse> Inclusions { get; set; } = Array.Empty<PackageInclusionResponse>();
    public IReadOnlyList<PackagePricingTierResponse> Pricing { get; set; } = Array.Empty<PackagePricingTierResponse>();
    public IReadOnlyList<PackageInventoryResponse> Inventory { get; set; } = Array.Empty<PackageInventoryResponse>();
    public IReadOnlyList<PackageMediaResponse> Media { get; set; } = Array.Empty<PackageMediaResponse>();
}
