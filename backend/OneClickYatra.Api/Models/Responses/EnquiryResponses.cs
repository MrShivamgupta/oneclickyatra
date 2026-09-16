namespace OneClickYatra.Api.Models.Responses;

public sealed class EnquiryResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public DateOnly? TravelDate { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
