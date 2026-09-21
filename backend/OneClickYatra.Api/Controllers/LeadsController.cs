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

    /// <summary>Searches leads with filtering (status, source, assigned agent, destination) and pagination, per <see cref="LeadSearchRequest"/>.</summary>
    [HttpGet]
    [HasPermission(PermissionConstants.LeadView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginationResponse<LeadResponse>>))]
    public async Task<ActionResult<ApiResponse<PaginationResponse<LeadResponse>>>> Search([FromQuery] LeadSearchRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.SearchAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<LeadResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Gets a single lead by id.</summary>
    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.LeadView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LeadResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Creates a new lead, starting in New status with a zero lead score. Validates that any
    /// supplied destination or assigned-to-user reference actually exists before saving.</summary>
    [HttpPost]
    [HasPermission(PermissionConstants.LeadCreate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LeadResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> Create(LeadRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead created."));
    }

    /// <summary>Updates a lead's contact/travel details. Validates that any supplied destination or
    /// assigned-to-user reference actually exists before saving. Does not itself change the lead's status.</summary>
    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LeadResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> Update(Guid id, LeadRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead updated."));
    }

    /// <summary>Moves a lead to a new pipeline stage (New/Contacted/QuotationSent/Negotiation/Confirmed/Lost).
    /// Unlike bookings, there is no fixed transition table here — any listed status is accepted from any other.</summary>
    [HttpPut("{id:guid}/status")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LeadResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> UpdateStatus(Guid id, LeadStatusRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.UpdateStatusAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead status updated."));
    }

    /// <summary>Assigns (or reassigns) a lead to a staff user. Validates that the target user exists.</summary>
    [HttpPut("{id:guid}/assign")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LeadResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> Assign(Guid id, LeadAssignRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.AssignAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead assigned."));
    }

    /// <summary>Sets a lead's numeric score (0-100), used to prioritize the pipeline.</summary>
    [HttpPut("{id:guid}/score")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<LeadResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<LeadResponse>>> UpdateScore(Guid id, LeadScoreRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.UpdateScoreAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<LeadResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead score updated."));
    }

    /// <summary>Converts a lead into a Customer CRM record, linking the two permanently. Idempotent: if
    /// the lead was already converted, returns the existing linked customer rather than creating a duplicate.</summary>
    [HttpPost("{id:guid}/convert-to-customer")]
    [HasPermission(PermissionConstants.LeadUpdate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<CustomerResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> ConvertToCustomer(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _leadAppFunction.ConvertToCustomerAsync(id, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Lead converted to customer."));
    }

    /// <summary>Permanently deletes a lead.</summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.LeadDelete)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _leadAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Lead deleted."));
    }

    /// <summary>Lists a lead's follow-ups. Note: does not verify the lead itself exists — an unknown
    /// leadId simply returns an empty list rather than 404.</summary>
    [HttpGet("{leadId:guid}/followups")]
    [HasPermission(PermissionConstants.FollowUpView)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IReadOnlyList<FollowUpResponse>>))]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FollowUpResponse>>>> ListFollowUps(Guid leadId, CancellationToken __cancellationToken)
    {
        var result = await _followUpAppFunction.ListByLeadAsync(leadId, __cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FollowUpResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    /// <summary>Schedules a new follow-up (Call/WhatsApp/Email/Visit) against a lead. Validates the lead exists.</summary>
    [HttpPost("{leadId:guid}/followups")]
    [HasPermission(PermissionConstants.FollowUpCreate)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<FollowUpResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object?>))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ApiResponse<object?>))]
    public async Task<ActionResult<ApiResponse<FollowUpResponse>>> CreateFollowUp(Guid leadId, FollowUpRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _followUpAppFunction.CreateAsync(leadId, __request, __cancellationToken);
        return Ok(ApiResponse<FollowUpResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Follow-up scheduled."));
    }
}
