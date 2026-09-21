using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.AppFunctions;

public class ReportAppFunctionTests
{
    private readonly Mock<IReportRepository> _reportRepository = new();

    private ReportAppFunction CreateSut() => new(_reportRepository.Object);

    [Fact]
    public async Task GetSalesAsync_NoDatesProvided_DefaultsToLast30Days()
    {
        DateOnly capturedFrom = default;
        DateOnly capturedTo = default;
        _reportRepository
            .Setup(r => r.GetSalesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .Callback<DateOnly, DateOnly, CancellationToken>((from, to, _) => { capturedFrom = from; capturedTo = to; })
            .ReturnsAsync(new List<SalesReportRowModel>());

        var sut = CreateSut();
        await sut.GetSalesAsync(new ReportDateRangeRequest(), CancellationToken.None);

        var expectedTo = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.Equal(expectedTo, capturedTo);
        Assert.Equal(expectedTo.AddDays(-30), capturedFrom);
    }

    [Fact]
    public async Task GetSalesAsync_BothDatesProvided_PassesThemThroughUnchanged()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);
        _reportRepository
            .Setup(r => r.GetSalesAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesReportRowModel>());

        var sut = CreateSut();
        await sut.GetSalesAsync(new ReportDateRangeRequest { FromDate = from, ToDate = to }, CancellationToken.None);

        _reportRepository.Verify(r => r.GetSalesAsync(from, to, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSalesAsync_MapsRepositoryRowsToResponse()
    {
        _reportRepository
            .Setup(r => r.GetSalesAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SalesReportRowModel>
            {
                new()
                {
                    BookingNumber = "BK-1",
                    CustomerName = "Jane Doe",
                    DestinationName = "Goa",
                    TotalAmount = 15000m,
                    Status = "Confirmed",
                    CreatedAt = new DateTime(2026, 1, 5),
                    AssignedAgentName = "Agent Smith"
                }
            });

        var sut = CreateSut();
        var result = await sut.GetSalesAsync(new ReportDateRangeRequest(), CancellationToken.None);

        var row = Assert.Single(result);
        Assert.Equal("BK-1", row.BookingNumber);
        Assert.Equal("Goa", row.DestinationName);
        Assert.Equal(15000m, row.TotalAmount);
        Assert.Equal("Agent Smith", row.AssignedAgentName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("quarter")]
    public async Task GetRevenueAsync_InvalidOrMissingGroupBy_DefaultsToDay(string? groupBy)
    {
        string? capturedGroupBy = null;
        _reportRepository
            .Setup(r => r.GetRevenueAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<DateOnly, DateOnly, string, CancellationToken>((_, _, g, _) => capturedGroupBy = g)
            .ReturnsAsync(new List<RevenueReportRowModel>());

        var sut = CreateSut();
        await sut.GetRevenueAsync(new ReportGroupedDateRangeRequest { GroupBy = groupBy }, CancellationToken.None);

        Assert.Equal("day", capturedGroupBy);
    }

    [Theory]
    [InlineData("week", "week")]
    [InlineData("MONTH", "month")]
    [InlineData("Day", "day")]
    public async Task GetRevenueAsync_ValidGroupBy_IsNormalizedToLowercaseAndPassedThrough(string groupBy, string expected)
    {
        string? capturedGroupBy = null;
        _reportRepository
            .Setup(r => r.GetRevenueAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<DateOnly, DateOnly, string, CancellationToken>((_, _, g, _) => capturedGroupBy = g)
            .ReturnsAsync(new List<RevenueReportRowModel>());

        var sut = CreateSut();
        await sut.GetRevenueAsync(new ReportGroupedDateRangeRequest { GroupBy = groupBy }, CancellationToken.None);

        Assert.Equal(expected, capturedGroupBy);
    }

    [Fact]
    public async Task GetOutstandingAsync_TakesNoDateRange_AndMapsRows()
    {
        _reportRepository
            .Setup(r => r.GetOutstandingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutstandingReportRowModel>
            {
                new() { BookingNumber = "BK-2", CustomerName = "John", TotalAmount = 1000m, AmountPaid = 400m, OutstandingAmount = 600m }
            });

        var sut = CreateSut();
        var result = await sut.GetOutstandingAsync(CancellationToken.None);

        var row = Assert.Single(result);
        Assert.Equal(600m, row.OutstandingAmount);
        _reportRepository.Verify(r => r.GetOutstandingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVendorPerformanceAsync_MapsRepositoryRowsToResponse()
    {
        _reportRepository
            .Setup(r => r.GetVendorPerformanceAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VendorPerformanceReportRowModel>
            {
                new() { VendorName = "Grand Palace Hotel", VendorType = "Hotel", RatingsCount = 2, AverageRating = 3.00m, PaymentsCount = 1, TotalPaid = 15000m }
            });

        var sut = CreateSut();
        var result = await sut.GetVendorPerformanceAsync(new ReportDateRangeRequest(), CancellationToken.None);

        var row = Assert.Single(result);
        Assert.Equal("Grand Palace Hotel", row.VendorName);
        Assert.Equal(3.00m, row.AverageRating);
        Assert.Equal(15000m, row.TotalPaid);
    }
}
