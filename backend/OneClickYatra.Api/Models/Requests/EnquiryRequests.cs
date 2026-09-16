using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class EnquiryRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public DateOnly? TravelDate { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class EnquiryStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class EnquirySearchRequest : PaginationRequest
{
    public string? Status { get; set; }
}
