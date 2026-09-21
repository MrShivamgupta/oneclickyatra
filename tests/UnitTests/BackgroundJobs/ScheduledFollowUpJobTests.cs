using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.BackgroundJobs;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.BackgroundJobs;

public class ScheduledFollowUpJobTests
{
    private readonly Mock<IFollowUpRepository> _followUpRepository = new();
    private readonly Mock<ILogger<ScheduledFollowUpJob>> _logger = new();

    private ScheduledFollowUpJob CreateSut() => new(_followUpRepository.Object, _logger.Object);

    [Fact]
    public async Task RunAsync_OverdueFollowUpsFound_LogsOneWarningPerItemAndSummary()
    {
        var overdue = new List<FollowUpModel>
        {
            new() { Id = Guid.NewGuid(), LeadId = Guid.NewGuid(), ScheduledAt = DateTime.UtcNow.AddDays(-2), Status = "Pending", Type = "Call", LeadCustomerName = "Alice" },
            new() { Id = Guid.NewGuid(), LeadId = Guid.NewGuid(), ScheduledAt = DateTime.UtcNow.AddDays(-1), Status = "Pending", Type = "Email", LeadCustomerName = "Bob" }
        };
        _followUpRepository.Setup(r => r.ListOverdueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(overdue);

        var sut = CreateSut();
        await sut.RunAsync(CancellationToken.None);

        VerifyLog(LogLevel.Warning, Times.Exactly(2));
        VerifyLog(LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task RunAsync_NoOverdueFollowUps_LogsSummaryOnlyNoWarning()
    {
        _followUpRepository.Setup(r => r.ListOverdueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<FollowUpModel>());

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
