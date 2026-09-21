using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using OneClickYatra.Api.BackgroundJobs;
using OneClickYatra.Api.Extensions;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Middleware;
using QuestPDF.Infrastructure;
using Serilog;

DapperTypeHandlers.RegisterAll();
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.AddAppSerilog();

builder.Services
    .AddControllers(options => options.Filters.AddService<ValidationActionFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddAppSwagger();
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);
builder.Services.AddAppRateLimiting(builder.Configuration);
builder.Services.AddAppHealthChecks(builder.Configuration);
builder.Services.AddAppHangfire(builder.Configuration);
builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TrackingIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "One Click Yatra API v1"));
}

// Development: no HTTPS redirect (local Angular calls http://localhost:5080; preflight breaks on redirect).
// Production behind nginx/Cloudflare: trust forwarded headers; TLS terminates at the edge.
var behindReverseProxy = builder.Configuration.GetValue("BehindReverseProxy", false);
if (behindReverseProxy)
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}
else if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// HSTS only makes sense once traffic is actually HTTPS (either terminated here, or terminated at
// the reverse proxy and forwarded as such via UseForwardedHeaders above) — never in local HTTP dev.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors(CorsExtensions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (builder.Configuration.GetValue("Hangfire:DashboardEnabled", true))
{
    app.UseHangfireDashboard(builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire");
}

RecurringJob.AddOrUpdate<TokenCleanupJob>(
    "token-cleanup",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<ReportGenerationJob>(
    "report-generation",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<PaymentReconciliationJob>(
    "payment-reconciliation",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<DepartureReminderJob>(
    "departure-reminder",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<PaymentReminderJob>(
    "payment-reminder",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<ScheduledFollowUpJob>(
    "scheduled-followup",
    job => job.RunAsync(CancellationToken.None),
    Cron.Daily);

try
{
    Log.Information("Starting One Click Yatra API");
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "One Click Yatra API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}
