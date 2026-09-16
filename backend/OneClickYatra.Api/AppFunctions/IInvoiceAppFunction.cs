using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IInvoiceAppFunction
{
    Task<PaginationResponse<InvoiceResponse>> SearchAsync(InvoiceSearchRequest __request, CancellationToken __cancellationToken);
    Task<InvoiceResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<byte[]> GeneratePdfAsync(Guid __id, CancellationToken __cancellationToken);
}
