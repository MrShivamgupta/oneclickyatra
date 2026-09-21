using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface IVendorInvoiceRepository
{
    Task CreateAsync(VendorInvoiceModel __invoice, CancellationToken __cancellationToken);
    Task<VendorInvoiceModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PaginationResponse<VendorInvoiceModel>> SearchByVendorIdAsync(Guid __vendorId, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<PaginationResponse<VendorInvoiceModel>> SearchAllAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken);
}
