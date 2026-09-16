using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OneClickYatra.Api.HealthChecks;

/// <summary>Lightweight liveness check with no external dependencies — used for the /health endpoint.</summary>
public sealed class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext __context, CancellationToken __cancellationToken = default) =>
        Task.FromResult(HealthCheckResult.Healthy("API process is running."));
}
