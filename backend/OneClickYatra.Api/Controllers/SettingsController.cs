using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;
using OneClickYatra.Api.Services.Email;
using OneClickYatra.Api.Services.Payments;
using OneClickYatra.Api.Services.WhatsApp;

namespace OneClickYatra.Api.Controllers;

/// <summary>Read-only integration status for the Settings page. Deliberately does not expose or
/// accept credential values -- those live in appsettings/env vars, not a DB-backed settings screen
/// (see IntegrationStatusResponse's doc comment for why).</summary>
[ApiController]
[Route("api/v1/settings")]
[Authorize]
public sealed class SettingsController : ControllerBase
{
    private readonly RazorpayOptions _razorpayOptions;
    private readonly WhatsAppOptions _whatsAppOptions;
    private readonly EmailOptions _emailOptions;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public SettingsController(
        IOptions<RazorpayOptions> __razorpayOptions,
        IOptions<WhatsAppOptions> __whatsAppOptions,
        IOptions<EmailOptions> __emailOptions,
        ITrackingIdAccessor __trackingIdAccessor)
    {
        _razorpayOptions = __razorpayOptions.Value;
        _whatsAppOptions = __whatsAppOptions.Value;
        _emailOptions = __emailOptions.Value;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet("integration-status")]
    [HasPermission(PermissionConstants.SettingsManage)]
    public ActionResult<ApiResponse<IntegrationStatusResponse>> GetIntegrationStatus()
    {
        var result = new IntegrationStatusResponse
        {
            RazorpayConfigured = _razorpayOptions.IsConfigured,
            WhatsAppConfigured = _whatsAppOptions.IsConfigured,
            EmailConfigured = _emailOptions.IsConfigured
        };
        return Ok(ApiResponse<IntegrationStatusResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }
}
