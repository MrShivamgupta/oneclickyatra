namespace OneClickYatra.Api.Models;

public sealed class BookingModel
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public Guid? LeadId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? QuotationId { get; set; }
    public Guid? QuotationOptionId { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? DestinationId { get; set; }
    public DateOnly? TravelDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public int NumberOfAdults { get; set; } = 1;
    public int NumberOfChildren { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Draft";
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public string? CustomerName { get; set; }
    public string? PackageTitle { get; set; }
    public string? DestinationName { get; set; }
    public string? DestinationImageUrl { get; set; }
}
