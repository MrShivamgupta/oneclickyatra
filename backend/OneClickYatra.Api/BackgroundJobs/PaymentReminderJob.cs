using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>
/// Recurring Hangfire job: emails a payment-due reminder to every customer on a Confirmed/InProgress
/// booking with an outstanding balance (AmountPaid &lt; TotalAmount) whose travel date is exactly
/// ReminderWindowDays away. Reuses BookingRepository.GetUpcomingDeparturesAsync (already built for the
/// dashboard's "upcoming departures" widget, and already reused by DepartureReminderJob) as a windowed
/// superset, then filters to an EXACT TravelDate match client-side for the same once-per-booking
/// idempotency reasoning as DepartureReminderJob -- a range would re-send the reminder on every run
/// until the balance clears. This job fires further out (10 days) than DepartureReminderJob (7 days)
/// so customers get a payment nudge before the "your trip is starting soon" reminder. No dedicated
/// balance-due-date schema exists yet, so the booking's own TravelDate is used as {{DueDate}} -- pay
/// before you travel is the natural deadline the seeded template's own copy already assumes ("helps us
/// confirm your hotel and travel arrangements without any last-minute hiccups"). This is distinct in
/// scope from PaymentReconciliationJob, which flags Pending (in-flight, unresolved) payment records --
/// a stuck-webhook signal, not a customer-facing balance-due reminder.
/// </summary>
public sealed class PaymentReminderJob
{
    private const int ReminderWindowDays = 10;
    private const int MaxBookingsToScan = 500;

    /// <summary>See BookingAppFunction's identical constant for why this is a shared placeholder
    /// literal rather than sourced from an agency-settings table (none exists yet).</summary>
    private const string SupportPhonePlaceholder = "+91-11-4567-8900";

    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IEmailNotificationSender _emailNotificationSender;
    private readonly ILogger<PaymentReminderJob> _logger;

    public PaymentReminderJob(
        IBookingRepository __bookingRepository,
        ICustomerRepository __customerRepository,
        IPackageRepository __packageRepository,
        IEmailNotificationSender __emailNotificationSender,
        ILogger<PaymentReminderJob> __logger)
    {
        _bookingRepository = __bookingRepository;
        _customerRepository = __customerRepository;
        _packageRepository = __packageRepository;
        _emailNotificationSender = __emailNotificationSender;
        _logger = __logger;
    }

    public async Task RunAsync(CancellationToken __cancellationToken = default)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(ReminderWindowDays);
        var withinWindow = await _bookingRepository.GetUpcomingDeparturesAsync(ReminderWindowDays, MaxBookingsToScan, __cancellationToken);
        var dueOnTargetDate = withinWindow.Where(b => b.TravelDate == targetDate && b.AmountPaid < b.TotalAmount).ToList();

        var sentCount = 0;
        var skippedCount = 0;

        foreach (var booking in dueOnTargetDate)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(booking.CustomerId, __cancellationToken);
                if (string.IsNullOrWhiteSpace(customer?.Email))
                {
                    _logger.LogWarning("Skipped PaymentReminder email for Booking {BookingId}: no email address on file for Customer {CustomerId}", booking.Id, booking.CustomerId);
                    skippedCount++;
                    continue;
                }

                var packageName = booking.PackageId.HasValue
                    ? (await _packageRepository.GetByIdAsync(booking.PackageId.Value, __cancellationToken))?.Title
                    : null;
                var dueAmount = booking.TotalAmount - booking.AmountPaid;

                var placeholders = new Dictionary<string, string>
                {
                    ["CustomerName"] = customer.FullName,
                    ["DueAmount"] = "Rs. " + dueAmount.ToString("N2"),
                    ["BookingReference"] = booking.BookingNumber,
                    ["PackageName"] = packageName ?? "your travel package",
                    ["DueDate"] = targetDate.ToString("dd MMM yyyy"),
                    // No customer-facing "resume payment" page exists yet -- payments are only ever
                    // initiated server-side via the bookings/payments API. Same documented-placeholder
                    // convention as QuotationAppFunction's QuotationLink.
                    ["PaymentLink"] = "#",
                    ["SupportPhone"] = SupportPhonePlaceholder
                };
                await _emailNotificationSender.SendAsync("PaymentReminder", customer.Email, placeholders, __cancellationToken);
                sentCount++;
            }
            catch (Exception exception)
            {
                // One booking's failure must never stop the rest of the batch.
                _logger.LogWarning(exception, "Failed to send PaymentReminder email for Booking {BookingId}", booking.Id);
                skippedCount++;
            }
        }

        _logger.LogInformation(
            "PaymentReminderJob completed for {TargetDate}: {FoundCount} bookings with outstanding balance found, {SentCount} reminders sent, {SkippedCount} skipped/failed.",
            targetDate, dueOnTargetDate.Count, sentCount, skippedCount);
    }
}
