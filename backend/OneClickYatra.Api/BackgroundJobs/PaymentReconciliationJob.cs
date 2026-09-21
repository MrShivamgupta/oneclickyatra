using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>Recurring Hangfire job: a local-data staleness check. It looks at Payments left in
/// "Pending" status (order created at the gateway, but never resolved to Paid/Failed) and flags the
/// ones that have sat there suspiciously long, which usually means the Razorpay webhook for that
/// order was never delivered or was dropped. This does NOT call out to the Razorpay API to
/// cross-check gateway-side status — no live gateway credentials exist in this environment, exactly
/// like every other Razorpay integration point in this codebase — it only reads what's already in
/// our own database, which is still a useful signal on its own.</summary>
public sealed class PaymentReconciliationJob
{
    private const string PendingStatus = "Pending";
    private const int PageSize = 100;

    /// <summary>Only payments created within this window are examined at all.</summary>
    private static readonly TimeSpan LookbackWindow = TimeSpan.FromHours(24);

    /// <summary>A Pending payment older than this, with no resolving webhook, is flagged as stuck.</summary>
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromHours(2);

    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentReconciliationJob> _logger;

    public PaymentReconciliationJob(IPaymentRepository __paymentRepository, ILogger<PaymentReconciliationJob> __logger)
    {
        _paymentRepository = __paymentRepository;
        _logger = __logger;
    }

    public async Task RunAsync(CancellationToken __cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var windowStart = now - LookbackWindow;
        var staleThreshold = now - StaleThreshold;

        var examinedCount = 0;
        var staleCount = 0;
        var pageNumber = 1;

        while (true)
        {
            var page = await _paymentRepository.SearchAsync(new PaymentSearchRequest
            {
                Status = PendingStatus,
                PageNumber = pageNumber,
                PageSize = PageSize
            }, __cancellationToken);

            if (page.Items.Count == 0)
            {
                break;
            }

            // Repository orders by CreatedAt DESC, so once a page contains a payment older than the
            // lookback window, every later page is older still — process what's in-window then stop.
            var inWindow = page.Items.Where(payment => payment.CreatedAt >= windowStart).ToList();
            examinedCount += inWindow.Count;

            foreach (var payment in inWindow.Where(payment => payment.CreatedAt <= staleThreshold))
            {
                staleCount++;
                _logger.LogWarning(
                    "PaymentReconciliationJob: payment {PaymentId} for booking {BookingNumber} has been Pending for {ElapsedHours:F1}h (CreatedAt: {CreatedAt:o}) with no resolving webhook; possible missed webhook.",
                    payment.Id, payment.BookingNumber ?? payment.BookingId.ToString(), (now - payment.CreatedAt).TotalHours, payment.CreatedAt);
            }

            var reachedOutOfWindowItem = inWindow.Count < page.Items.Count;
            var reachedLastPage = (long)pageNumber * PageSize >= page.TotalCount;
            if (reachedOutOfWindowItem || reachedLastPage)
            {
                break;
            }

            pageNumber++;
        }

        _logger.LogInformation(
            "PaymentReconciliationJob completed. ExaminedPendingInLast24h: {ExaminedCount}, StaleBeyond2h: {StaleCount}.",
            examinedCount, staleCount);
    }
}
