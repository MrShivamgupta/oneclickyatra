using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IFeedbackRepository
{
    Task<FeedbackModel?> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task CreateAsync(FeedbackModel __feedback, CancellationToken __cancellationToken);
    Task<PaginationResponse<FeedbackModel>> SearchAsync(FeedbackSearchRequest __request, CancellationToken __cancellationToken);

    /// <summary>Recent, well-rated, commented feedback for the public home page -- not paginated,
    /// just the most recent <paramref name="__limit"/> that clear the bar.</summary>
    Task<IReadOnlyList<FeedbackModel>> GetPublicTestimonialsAsync(int __minRating, int __limit, CancellationToken __cancellationToken);
}
