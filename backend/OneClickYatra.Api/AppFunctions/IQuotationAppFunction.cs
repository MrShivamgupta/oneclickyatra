using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IQuotationAppFunction
{
    Task<PaginationResponse<QuotationResponse>> SearchAsync(QuotationSearchRequest __request, CancellationToken __cancellationToken);
    Task<QuotationDetailResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<QuotationResponse> CreateAsync(QuotationRequest __request, CancellationToken __cancellationToken);
    Task<QuotationResponse> UpdateAsync(Guid __id, QuotationRequest __request, CancellationToken __cancellationToken);
    Task<QuotationResponse> RegenerateLinkAsync(Guid __id, CancellationToken __cancellationToken);
    Task<QuotationResponse> SendAsync(Guid __id, CancellationToken __cancellationToken);
    Task<List<QuotationOptionResponse>> ReplaceOptionsAsync(Guid __id, List<QuotationOptionRequest> __options, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);

    Task<QuotationPublicResponse> GetPublicAsync(string __token, CancellationToken __cancellationToken);
    Task ApprovePublicAsync(string __token, QuotationPublicApproveRequest __request, string? __ipAddress, CancellationToken __cancellationToken);
    Task RejectPublicAsync(string __token, QuotationPublicRejectRequest __request, string? __ipAddress, CancellationToken __cancellationToken);

    Task<byte[]> GeneratePdfAsync(Guid __id, CancellationToken __cancellationToken);
    Task<byte[]> GeneratePublicPdfAsync(string __token, CancellationToken __cancellationToken);
}
