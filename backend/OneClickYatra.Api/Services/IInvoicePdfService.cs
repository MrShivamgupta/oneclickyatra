using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Services;

public interface IInvoicePdfService
{
    byte[] Generate(InvoiceModel __invoice);
}
