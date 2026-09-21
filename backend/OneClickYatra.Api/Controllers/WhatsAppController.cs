using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/whatsapp")]
[Authorize]
public sealed class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppAppFunction _whatsAppAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public WhatsAppController(IWhatsAppAppFunction __whatsAppAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _whatsAppAppFunction = __whatsAppAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet("templates")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<WhatsAppTemplateResponse>>>> SearchTemplates([FromQuery] WhatsAppTemplateSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _whatsAppAppFunction.SearchTemplatesAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<WhatsAppTemplateResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("templates/{id:guid}")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<WhatsAppTemplateResponse>>> GetTemplateById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _whatsAppAppFunction.GetTemplateByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<WhatsAppTemplateResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("templates")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<WhatsAppTemplateResponse>>> CreateTemplate(WhatsAppTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _whatsAppAppFunction.CreateTemplateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<WhatsAppTemplateResponse>.Ok(result, _trackingIdAccessor.TrackingId, "WhatsApp template created."));
    }

    [HttpPut("templates/{id:guid}")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<WhatsAppTemplateResponse>>> UpdateTemplate(Guid id, WhatsAppTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _whatsAppAppFunction.UpdateTemplateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<WhatsAppTemplateResponse>.Ok(result, _trackingIdAccessor.TrackingId, "WhatsApp template updated."));
    }

    [HttpDelete("templates/{id:guid}")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteTemplate(Guid id, CancellationToken __cancellationToken)
    {
        await _whatsAppAppFunction.DeleteTemplateAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "WhatsApp template deleted."));
    }

    [HttpPost("send")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<NotificationLogResponse>>> Send(WhatsAppSendMessageRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _whatsAppAppFunction.SendMessageAsync(__request, __cancellationToken);
        return Ok(ApiResponse<NotificationLogResponse>.Ok(result, _trackingIdAccessor.TrackingId, "WhatsApp message sent."));
    }

    [HttpGet("logs")]
    [HasPermission(PermissionConstants.WhatsAppManage)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<NotificationLogResponse>>>> SearchLogs([FromQuery] NotificationLogSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _whatsAppAppFunction.SearchNotificationLogsAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<NotificationLogResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Public: Meta's one-time GET verification handshake when the webhook subscription is
    /// configured in the Meta App dashboard. Echoes back hub.challenge only if hub.verify_token
    /// matches this environment's configured WebhookVerifyToken.</summary>
    [HttpGet("webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public ActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (!_whatsAppAppFunction.VerifyWebhookChallenge(mode, verifyToken) || string.IsNullOrEmpty(challenge))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Content(challenge, "text/plain");
    }

    /// <summary>Public: called by Meta's servers, never by a browser. Trust comes entirely from the
    /// X-Hub-Signature-256 HMAC, not from a JWT — there is no user to authenticate here.</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public async Task<IActionResult> Webhook(CancellationToken __cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(__cancellationToken);

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();

        await _whatsAppAppFunction.ProcessWebhookAsync(rawBody, signature, __cancellationToken);
        return Ok();
    }
}
