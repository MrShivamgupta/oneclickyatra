using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Services;

public interface IInvoicePdfService
{
    Task<byte[]> GenerateAsync(InvoiceModel __invoice, CancellationToken __cancellationToken);
}
