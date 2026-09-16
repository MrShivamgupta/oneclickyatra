namespace OneClickYatra.Api.Models;

public sealed class PackagePricingTierModel
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public string? HotelCategory { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal? ChildPrice { get; set; }
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string Currency { get; set; } = "INR";
}
