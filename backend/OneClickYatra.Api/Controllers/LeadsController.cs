using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Security.Authorization;

namespace OneClickYatra.Api.Controllers;

[ApiController]
[Route("api/v1/leads")]
[Authorize]
public sealed class LeadsController : ControllerBase
{
    private readonly ILeadAppFunction _leadAppFunction;
    private readonly IFollowUpAppFunction _followUpAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public LeadsController(ILeadAppFunction __leadAppFunction, IFollowUpAppFunction __followUpAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _leadAppFunction = __leadAppFunction;
        _followUpAppFunction = __followUpAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.LeadView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<LeadResponse>>>> Search([FromQuery] LeadSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<LeadResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.LeadView)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.LeadCreate)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> Create(LeadRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> Update(Guid id, LeadRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead updated."));
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> UpdateStatus(Guid id, LeadStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead status updated."));
    }

    [HttpPut("{id:guid}/assign")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> Assign(Guid id, LeadAssignRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.AssignAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead assigned."));
    }

    [HttpPut("{id:guid}/score")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> UpdateScore(Guid id, LeadScoreRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.UpdateScoreAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead score updated."));
    }

    [HttpPost("{id:guid}/convert-to-customer")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> ConvertToCustomer(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.ConvertToCustomerAsync(id, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead converted to customer."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.LeadDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _leadAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Lead deleted."));
    }

    [HttpGet("{leadId:guid}/followups")]
    [HasPermission(PermissionConstants.FollowUpView)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FollowUpResponse>>>> ListFollowUps(Guid leadId, CancellationToken __cancellationToken)
    {
        var result = await _followUpAppFunction.ListByLeadAsync(leadId, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FollowUpResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost("{leadId:guid}/followups")]
    [HasPermission(PermissionConstants.FollowUpCreate)]
    public async Task<ActionResult<ApiResponse<FollowUpResponse>>> CreateFollowUp(Guid leadId, FollowUpRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _followUpAppFunction.CreateAsync(leadId, __request, __cancellationToken);
        return Ok(ApiResponse<FollowUpResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Follow-up scheduled."));
    }
}
