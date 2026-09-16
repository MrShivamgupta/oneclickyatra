using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IInvoiceRepository
{
    Task<InvoiceModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<InvoiceModel?> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken);
    Task<PaginationResponse<InvoiceModel>> SearchAsync(InvoiceSearchRequest __request, CancellationToken __cancellationToken);
    Task CreateAsync(InvoiceModel __invoice, CancellationToken __cancellationToken);
}
