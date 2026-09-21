using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class VendorRequest
{
    public string Name { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class VendorSearchRequest : PaginationRequest
{
    public string? VendorType { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class VendorContactRequest
{
    public string ContactName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class VendorRateRequest
{
    public Guid? DestinationId { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public decimal RateAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public DateOnly? ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}

public sealed class VendorPaymentRequest
{
    public Guid? BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public sealed class VendorPaymentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class VendorPerformanceRequest
{
    public Guid? BookingId { get; set; }
    public int Rating { get; set; }
    public string? Notes { get; set; }
}

public sealed class VendorLinkUserRequest
{
    public Guid UserId { get; set; }
}
