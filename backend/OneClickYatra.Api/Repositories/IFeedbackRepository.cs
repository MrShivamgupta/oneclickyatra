using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IFeedbackRepository
{
    Task<FeedbackModel?> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task CreateAsync(FeedbackModel __feedback, CancellationToken __cancellationToken);
    Task<PaginationResponse<FeedbackModel>> SearchAsync(FeedbackSearchRequest __request, CancellationToken __cancellationToken);
}
