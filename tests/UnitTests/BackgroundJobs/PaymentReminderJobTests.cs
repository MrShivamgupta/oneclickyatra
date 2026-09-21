using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.BackgroundJobs;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.UnitTests.BackgroundJobs;

public class PaymentReminderJobTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IPackageRepository> _packageRepository = new();
    private readonly Mock<IEmailNotificationSender> _emailNotificationSender = new();
    private readonly Mock<ILogger<PaymentReminderJob>> _logger = new();

    private PaymentReminderJob CreateSut() => new(_bookingRepository.Object, _customerRepository.Object, _packageRepository.Object, _emailNotificationSender.Object, _logger.Object);

    [Fact]
    public async Task RunAsync_BookingWithOutstandingBalanceExactlyTenDaysOut_SendsOneReminder()
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var customerId = Guid.NewGuid();
        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BK-1",
            CustomerId = customerId,
            TravelDate = targetDate,
            TotalAmount = 50000m,
            AmountPaid = 20000m
        };
        _bookingRepository.Setup(r => r.GetUpcomingDeparturesAsync(10, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([booking]);
        _customerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerModel { Id = customerId, FullName = "Priya", Email = "priya@example.com" });

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        _emailNotificationSender.Verify(s => s.SendAsync("PaymentReminder", "priya@example.com", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_BookingFullyPaid_IsExcluded()
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BK-2",
            CustomerId = Guid.NewGuid(),
            TravelDate = targetDate,
            TotalAmount = 50000m,
            AmountPaid = 50000m
        };
        _bookingRepository.Setup(r => r.GetUpcomingDeparturesAsync(10, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([booking]);

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        _emailNotificationSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
        _customerRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_BookingDepartingInThreeDays_IsExcludedNotExactlyTenDayMatch()
    {
        var nearDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3);
        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BK-3",
            CustomerId = Guid.NewGuid(),
            TravelDate = nearDate,
            TotalAmount = 50000m,
            AmountPaid = 0m
        };
        _bookingRepository.Setup(r => r.GetUpcomingDeparturesAsync(10, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([booking]);

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        _emailNotificationSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_CustomerHasNoEmailOnFile_SkipsWithoutThrowing()
    {
        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var customerId = Guid.NewGuid();
        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            BookingNumber = "BK-4",
            CustomerId = customerId,
            TravelDate = targetDate,
            TotalAmount = 50000m,
            AmountPaid = 10000m
        };
        _bookingRepository.Setup(r => r.GetUpcomingDeparturesAsync(10, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync([booking]);
        _customerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerModel { Id = customerId, FullName = "No Email", Email = null });

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        _emailNotificationSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
