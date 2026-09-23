using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class InvoiceAppFunction : IInvoiceAppFunction
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoicePdfService _pdfService;
    private readonly ILogger<InvoiceAppFunction> _logger;

    public InvoiceAppFunction(IInvoiceRepository __invoiceRepository, IInvoicePdfService __pdfService, ILogger<InvoiceAppFunction> __logger)
    {
        _invoiceRepository = __invoiceRepository;
        _pdfService = __pdfService;
        _logger = __logger;
    }

    public async Task<PaginationResponse<InvoiceResponse>> SearchAsync(InvoiceSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _invoiceRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<InvoiceResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Invoice", __id);
        return ToResponse(invoice);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Invoice", __id);

        try
        {
            return await _pdfService.GenerateAsync(invoice, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to generate PDF for {InvoiceId}", __id);
            throw;
        }
    }

    private static InvoiceResponse ToResponse(OneClickYatra.Api.Models.InvoiceModel invoice) => new()
    {
        Id = invoice.Id,
        BookingId = invoice.BookingId,
        BookingNumber = invoice.BookingNumber,
        CustomerName = invoice.CustomerName,
        InvoiceNumber = invoice.InvoiceNumber,
        Amount = invoice.Amount,
        IssuedAt = invoice.IssuedAt
    };
}
