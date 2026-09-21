using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>
/// Recurring Hangfire job: emails a departure reminder to every customer whose booking departs in
/// exactly 7 days. Reuses BookingRepository.GetUpcomingDeparturesAsync (already built for the
/// dashboard's "upcoming departures" widget) as a 7-day-window superset, then filters to an EXACT
/// TravelDate match client-side — an exact match (not "within the next 7 days") is what keeps this
/// idempotent day over day; a range would re-send the same reminder on every one of the 7 days
/// leading up to departure.
/// </summary>
public sealed class DepartureReminderJob
{
    private const int ReminderWindowDays = 7;
    private const int MaxDeparturesToScan = 500;

    /// <summary>See AppFunctions/BookingAppFunction's identical constant for why this is a shared
    /// placeholder literal rather than sourced from an agency-settings table (none exists yet).</summary>
    private const string SupportPhonePlaceholder = "+91-11-4567-8900";

    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IEmailNotificationSender _emailNotificationSender;
    private readonly ILogger<DepartureReminderJob> _logger;

    public DepartureReminderJob(
        IBookingRepository __bookingRepository,
        ICustomerRepository __customerRepository,
        IDestinationRepository __destinationRepository,
        IPackageRepository __packageRepository,
        IEmailNotificationSender __emailNotificationSender,
        ILogger<DepartureReminderJob> __logger)
    {
        _bookingRepository = __bookingRepository;
        _customerRepository = __customerRepository;
        _destinationRepository = __destinationRepository;
        _packageRepository = __packageRepository;
        _emailNotificationSender = __emailNotificationSender;
        _logger = __logger;
    }

    public async Task RunAsync(CancellationToken __cancellationToken = default)
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(ReminderWindowDays);
        var withinWindow = await _bookingRepository.GetUpcomingDeparturesAsync(ReminderWindowDays, MaxDeparturesToScan, __cancellationToken);
        var departingOnTargetDate = withinWindow.Where(b => b.TravelDate == targetDate).ToList();

        var sentCount = 0;
        var skippedCount = 0;

        foreach (var booking in departingOnTargetDate)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(booking.CustomerId, __cancellationToken);
                if (string.IsNullOrWhiteSpace(customer?.Email))
                {
                    _logger.LogWarning("Skipped DepartureReminder email for Booking {BookingId}: no email address on file for Customer {CustomerId}", booking.Id, booking.CustomerId);
                    skippedCount++;
                    continue;
                }

                var destinationName = booking.DestinationId.HasValue
                    ? (await _destinationRepository.GetByIdAsync(booking.DestinationId.Value, __cancellationToken))?.Name
                    : null;
                var packageName = booking.PackageId.HasValue
                    ? (await _packageRepository.GetByIdAsync(booking.PackageId.Value, __cancellationToken))?.Title
                    : null;

                var placeholders = new Dictionary<string, string>
                {
                    ["CustomerName"] = customer.FullName,
                    ["DestinationName"] = destinationName ?? "your destination",
                    ["PackageName"] = packageName ?? "your travel package",
                    ["DepartureDate"] = targetDate.ToString("dd MMM yyyy"),
                    ["DaysToDeparture"] = ReminderWindowDays.ToString(),
                    ["SupportPhone"] = SupportPhonePlaceholder
                };
                await _emailNotificationSender.SendAsync("DepartureReminder", customer.Email, placeholders, __cancellationToken);
                sentCount++;
            }
            catch (Exception exception)
            {
                // One booking's failure must never stop the rest of the batch.
                _logger.LogWarning(exception, "Failed to send DepartureReminder email for Booking {BookingId}", booking.Id);
                skippedCount++;
            }
        }

        _logger.LogInformation(
            "DepartureReminderJob completed for {TargetDate}: {FoundCount} bookings found, {SentCount} reminders sent, {SkippedCount} skipped/failed.",
            targetDate, departingOnTargetDate.Count, sentCount, skippedCount);
    }
}
