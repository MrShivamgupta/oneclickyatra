using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

/// <summary>Admin CRUD for the outbound notification email templates -- the part of the "Settings"
/// screen that's actually a real, working subsystem (see EmailTemplateService, which every
/// notification trigger already renders through).</summary>
[ApiController]
[Route("api/v1/email-templates")]
[Authorize]
public sealed class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateAppFunction _emailTemplateAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public EmailTemplatesController(IEmailTemplateAppFunction __emailTemplateAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _emailTemplateAppFunction = __emailTemplateAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<EmailTemplateResponse>>>> Search([FromQuery] EmailTemplateSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _emailTemplateAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<EmailTemplateResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<EmailTemplateResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _emailTemplateAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<EmailTemplateResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<EmailTemplateResponse>>> Create(UpsertEmailTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _emailTemplateAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<EmailTemplateResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Email template created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<EmailTemplateResponse>>> Update(Guid id, UpsertEmailTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _emailTemplateAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<EmailTemplateResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Email template updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.SettingsManage)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _emailTemplateAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Email template deleted."));
    }
}
