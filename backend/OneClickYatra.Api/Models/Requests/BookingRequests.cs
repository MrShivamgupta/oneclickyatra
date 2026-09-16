using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class BookingRequest
{
    public Guid CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? PackageId { get; set; }
    public Guid? DestinationId { get; set; }
    public DateOnly? TravelDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public int NumberOfAdults { get; set; } = 1;
    public int NumberOfChildren { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
}

public sealed class BookingSearchRequest : PaginationRequest
{
    public string? Status { get; set; }
    public Guid? CustomerId { get; set; }
}

public sealed class BookingStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public sealed class BookingCancelRequest
{
    public string? Reason { get; set; }
}

public sealed class BookingRefundRequest
{
    public string? Reason { get; set; }
}

public sealed class BookingPassengerRequest
{
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public bool IsLeadPassenger { get; set; }
}

public sealed class BookingAddOnRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}
