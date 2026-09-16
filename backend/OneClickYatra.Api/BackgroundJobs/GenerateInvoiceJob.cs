using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>Enqueued by the payment webhook handler once a booking is fully paid. Idempotent —
/// safe to enqueue more than once for the same booking (Hangfire retries included).</summary>
public sealed class GenerateInvoiceJob
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<GenerateInvoiceJob> _logger;

    public GenerateInvoiceJob(IInvoiceRepository __invoiceRepository, IBookingRepository __bookingRepository, ILogger<GenerateInvoiceJob> __logger)
    {
        _invoiceRepository = __invoiceRepository;
        _bookingRepository = __bookingRepository;
        _logger = __logger;
    }

    public async Task RunAsync(Guid __bookingId, CancellationToken __cancellationToken = default)
    {
        var existing = await _invoiceRepository.GetByBookingIdAsync(__bookingId, __cancellationToken);
        if (existing is not null)
        {
            _logger.LogInformation("GenerateInvoiceJob: invoice already exists for booking {BookingId}; skipping.", __bookingId);
            return;
        }

        var booking = await _bookingRepository.GetByIdAsync(__bookingId, __cancellationToken);
        if (booking is null)
        {
            _logger.LogWarning("GenerateInvoiceJob: booking {BookingId} not found.", __bookingId);
            return;
        }

        var invoice = new InvoiceModel
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            Amount = booking.TotalAmount,
            IssuedAt = DateTime.UtcNow
        };

        await _invoiceRepository.CreateAsync(invoice, __cancellationToken);
        _logger.LogInformation("GenerateInvoiceJob: generated invoice {InvoiceNumber} for booking {BookingId}.", invoice.InvoiceNumber, booking.Id);
    }
}
