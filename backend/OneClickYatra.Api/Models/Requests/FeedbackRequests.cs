using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class FeedbackRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public sealed class FeedbackSearchRequest : PaginationRequest
{
    public int? MinRating { get; set; }
    public int? MaxRating { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
