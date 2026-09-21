namespace OneClickYatra.Api.Models;

public sealed class VendorModel
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
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class VendorContactModel
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class VendorRateModel
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public decimal RateAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class VendorPaymentModel
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class VendorPerformanceModel
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
    public Guid? RecordedBy { get; set; }
    public string? RecordedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// A best-effort, read-only projection for the Vendor Portal's "booking requests" feed.
/// There is no direct Bookings-to-Vendor link in the schema, so this joins Bookings to
/// VendorRates on DestinationId as the closest reasonable approximation of "bookings this
/// vendor might be involved in" — see VendorPortalAppFunction.GetMyBookingRequestsAsync.
/// </summary>
public sealed class VendorBookingRequestModel
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
