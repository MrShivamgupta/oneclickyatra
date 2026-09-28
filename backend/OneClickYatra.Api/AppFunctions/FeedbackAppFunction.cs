using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// Thin pass-through over <see cref="IFeedbackRepository"/>'s search — the admin read-side for
/// customer feedback (write-side lives in CustomerPortalAppFunction.SubmitFeedbackAsync, which this
/// never touches). No business-rule branching here, just a 1:1 row mapping, following
/// ReportAppFunction's minimal style.
/// </summary>
public sealed class FeedbackAppFunction : IFeedbackAppFunction
{
    private const int PublicTestimonialMinRating = 4;
    private const int PublicTestimonialLimit = 6;

    private readonly IFeedbackRepository _feedbackRepository;

    public FeedbackAppFunction(IFeedbackRepository __feedbackRepository)
    {
        _feedbackRepository = __feedbackRepository;
    }

    public async Task<PaginationResponse<FeedbackListResponse>> SearchAsync(FeedbackSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _feedbackRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<FeedbackListResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<IReadOnlyList<TestimonialResponse>> GetPublicTestimonialsAsync(CancellationToken __cancellationToken)
    {
        var feedback = await _feedbackRepository.GetPublicTestimonialsAsync(PublicTestimonialMinRating, PublicTestimonialLimit, __cancellationToken);
        return feedback.Select(f => new TestimonialResponse
        {
            CustomerName = f.CustomerName ?? "One Click Yatra traveller",
            Rating = f.Rating,
            Comment = f.Comment ?? string.Empty,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    private static FeedbackListResponse ToResponse(FeedbackModel feedback) => new()
    {
        Id = feedback.Id,
        BookingNumber = feedback.BookingNumber,
        CustomerName = feedback.CustomerName,
        Rating = feedback.Rating,
        Comment = feedback.Comment,
        CreatedAt = feedback.CreatedAt
    };
}
