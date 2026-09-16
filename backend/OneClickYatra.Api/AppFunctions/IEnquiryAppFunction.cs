using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IEnquiryAppFunction
{
    Task<PaginationResponse<EnquiryResponse>> SearchAsync(EnquirySearchRequest __request, CancellationToken __cancellationToken);
    Task<EnquiryResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<EnquiryResponse> CreateAsync(EnquiryRequest __request, CancellationToken __cancellationToken);
    Task<EnquiryResponse> UpdateStatusAsync(Guid __id, EnquiryStatusRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
