using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class PackageRequest
{
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
}

public sealed class PackageSearchRequest : PaginationRequest
{
    public Guid? DestinationId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Status { get; set; }
}

public sealed class PackageStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class PackageItineraryDayRequest
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class PackageInclusionRequest
{
    public string Description { get; set; } = string.Empty;
    public bool IsIncluded { get; set; } = true;
    public int SortOrder { get; set; }
}

public sealed class PackagePricingTierRequest
{
    public string TierName { get; set; } = string.Empty;
    public string? HotelCategory { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal? ChildPrice { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string Currency { get; set; } = "INR";
}

public sealed class PackageInventoryRequest
{
    public DateOnly DepartureDate { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public string Status { get; set; } = "Open";
}

public sealed class PackageMediaRequest
{
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Image";
    public int SortOrder { get; set; }
    public bool IsCoverImage { get; set; }
}
