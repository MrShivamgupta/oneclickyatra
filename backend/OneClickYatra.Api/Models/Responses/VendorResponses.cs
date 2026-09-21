namespace OneClickYatra.Api.Models.Responses;

public sealed class VendorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public Guid? UserId { get; set; }
    public decimal? Rating { get; set; }
    public bool IsActive { get; set; }
}

public sealed class VendorContactResponse
{
    public Guid? Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class VendorRateResponse
{
    public Guid? Id { get; set; }
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public decimal RateAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}

public sealed class VendorPaymentResponse
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class VendorPerformanceResponse
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>See VendorBookingRequestModel for the scope note on this best-effort projection.</summary>
public sealed class VendorBookingRequestResponse
{
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? TravelDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
}
