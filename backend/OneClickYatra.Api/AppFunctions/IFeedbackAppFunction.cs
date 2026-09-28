using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IFeedbackAppFunction
{
    Task<PaginationResponse<FeedbackListResponse>> SearchAsync(FeedbackSearchRequest __request, CancellationToken __cancellationToken);
    Task<IReadOnlyList<TestimonialResponse>> GetPublicTestimonialsAsync(CancellationToken __cancellationToken);
}
