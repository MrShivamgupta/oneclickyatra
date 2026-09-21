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
