namespace OneClickYatra.Api.Models.Responses;

public sealed class BookingResponse
{
    public Guid Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public Guid? LeadId { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? QuotationId { get; set; }
    public Guid? QuotationOptionId { get; set; }
    public Guid? PackageId { get; set; }
    public string? PackageTitle { get; set; }
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public DateOnly? TravelDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
}

public sealed class BookingPassengerResponse
{
    public Guid? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public bool IsLeadPassenger { get; set; }
}

public sealed class BookingAddOnResponse
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public sealed class BookingDetailResponse
{
    public BookingResponse Booking { get; set; } = new();
    public List<BookingPassengerResponse> Passengers { get; set; } = [];
    public List<BookingAddOnResponse> AddOns { get; set; } = [];
}

public sealed class BookingStatusHistoryResponse
{
    public Guid Id { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
}
