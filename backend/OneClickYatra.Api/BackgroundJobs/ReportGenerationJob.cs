using OneClickYatra.Api.Repositories;

namespace OneClickYatra.Api.BackgroundJobs;

/// <summary>Recurring Hangfire job: computes yesterday's sales/revenue snapshot and writes it to the
/// log stream. No recipient or storage target was specified in the SRS for this report yet, so a
/// structured Information log line is the full deliverable for this pass — not an email or a new
/// report-history table.</summary>
public sealed class ReportGenerationJob
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<ReportGenerationJob> _logger;

    public ReportGenerationJob(IReportRepository __reportRepository, ILogger<ReportGenerationJob> __logger)
    {
        _reportRepository = __reportRepository;
        _logger = __logger;
    }

    public async Task RunAsync(CancellationToken __cancellationToken = default)
    {
        var reportDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var salesRows = await _reportRepository.GetSalesAsync(reportDate, reportDate, __cancellationToken);
        var revenueRows = await _reportRepository.GetRevenueAsync(reportDate, reportDate, "day", __cancellationToken);

        var totalBookings = salesRows.Count;
        var totalRevenue = revenueRows.Sum(row => row.Revenue);

        _logger.LogInformation(
            "ReportGenerationJob: daily snapshot for {ReportDate}. TotalBookings: {TotalBookings}, TotalRevenue: {TotalRevenue}.",
            reportDate, totalBookings, totalRevenue);
    }
}
