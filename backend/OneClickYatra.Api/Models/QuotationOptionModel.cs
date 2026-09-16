namespace OneClickYatra.Api.Models;

public sealed class QuotationOptionModel
{
    public Guid Id { get; set; }
    public Guid QuotationId { get; set; }
    public Guid? PackageId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public int? DurationDays { get; set; }
    public int? DurationNights { get; set; }
    public string? HotelCategory { get; set; }
    public int NumberOfPeople { get; set; } = 1;
    public decimal PricePerPerson { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsRecommended { get; set; }
    public int SortOrder { get; set; }

    public string? DestinationName { get; set; }
}
