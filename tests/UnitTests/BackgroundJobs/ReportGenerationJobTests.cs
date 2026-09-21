using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.BackgroundJobs;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.BackgroundJobs;

public class ReportGenerationJobTests
{
    private readonly Mock<IReportRepository> _reportRepository = new();
    private readonly Mock<ILogger<ReportGenerationJob>> _logger = new();

    private ReportGenerationJob CreateSut() => new(_reportRepository.Object, _logger.Object);

    [Fact]
    public async Task RunAsync_SalesAndRevenueForYesterday_LogsSnapshotSummary()
    {
        var salesRows = new List<SalesReportRowModel>
        {
            new() { BookingNumber = "BK-1", CustomerName = "Alice", TotalAmount = 1000m, Status = "Confirmed", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { BookingNumber = "BK-2", CustomerName = "Bob", TotalAmount = 2500m, Status = "Confirmed", CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };
        var revenueRows = new List<RevenueReportRowModel>
        {
            new() { PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), Revenue = 3500m }
        };

        _reportRepository
            .Setup(r => r.GetSalesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(salesRows);
        _reportRepository
            .Setup(r => r.GetRevenueAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), "day", It.IsAny<CancellationToken>()))
            .ReturnsAsync(revenueRows);

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        _reportRepository.Verify(r => r.GetSalesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Once);
        _reportRepository.Verify(r => r.GetRevenueAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), "day", It.IsAny<CancellationToken>()), Times.Once);
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
