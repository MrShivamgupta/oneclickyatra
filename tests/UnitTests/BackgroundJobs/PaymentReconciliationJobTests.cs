using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.BackgroundJobs;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.BackgroundJobs;

public class PaymentReconciliationJobTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<ILogger<PaymentReconciliationJob>> _logger = new();

    private PaymentReconciliationJob CreateSut() => new(_paymentRepository.Object, _logger.Object);

    [Fact]
    public async Task RunAsync_OnePaymentStaleBeyondTwoHours_LogsOneWarningAndSummary()
    {
        var now = DateTime.UtcNow;
        var freshPayment = new PaymentModel
        {
            Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), BookingNumber = "BK-FRESH",
            Status = "Pending", CreatedAt = now.AddMinutes(-30)
        };
        var stalePayment = new PaymentModel
        {
            Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), BookingNumber = "BK-STALE",
            Status = "Pending", CreatedAt = now.AddHours(-3)
        };
        var outOfWindowPayment = new PaymentModel
        {
            Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), BookingNumber = "BK-OLD",
            Status = "Pending", CreatedAt = now.AddHours(-30)
        };

        var items = new List<PaymentModel> { freshPayment, stalePayment, outOfWindowPayment };
        _paymentRepository
            .Setup(r => r.SearchAsync(It.Is<PaymentSearchRequest>(req => req.Status == "Pending"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaginationResponse<PaymentModel>.Create(items, 1, 100, items.Count));

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        // Only the payment stale beyond the 2h threshold (and within the 24h lookback) warrants a warning.
        VerifyLog(LogLevel.Warning, Times.Once());
        VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task RunAsync_NoStalePayments_LogsSummaryOnlyNoWarning()
    {
        var now = DateTime.UtcNow;
        var freshPayment = new PaymentModel
        {
            Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), BookingNumber = "BK-FRESH",
            Status = "Pending", CreatedAt = now.AddMinutes(-10)
        };

        _paymentRepository
            .Setup(r => r.SearchAsync(It.IsAny<PaymentSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaginationResponse<PaymentModel>.Create(new List<PaymentModel> { freshPayment }, 1, 100, 1));

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        VerifyLog(LogLevel.Warning, Times.Never());
        VerifyLog(LogLevel.Information, Times.Once());
    }

    private void VerifyLog(LogLevel __level, Times __times)
    {
        _logger.Verify(
            x => x.Log(
                __level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            __times);
    }
}
