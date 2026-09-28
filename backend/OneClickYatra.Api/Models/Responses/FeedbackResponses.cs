namespace OneClickYatra.Api.Models.Responses;

public sealed class FeedbackResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public string? CustomerName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class FeedbackListResponse
{
    public Guid Id { get; set; }
    public string? BookingNumber { get; set; }
    public string? CustomerName { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Public-safe testimonial for the home page -- deliberately omits BookingId/CustomerId/
/// BookingNumber, none of which a public visitor needs or should see.</summary>
public sealed class TestimonialResponse
{
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
