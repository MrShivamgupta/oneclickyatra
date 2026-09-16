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
[Route("api/v1/customers")]
[Authorize]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerAppFunction _customerAppFunction;
    private readonly ITrackingIdAccessor _trackingIdAccessor;

    public CustomersController(ICustomerAppFunction __customerAppFunction, ITrackingIdAccessor __trackingIdAccessor)
    {
        _customerAppFunction = __customerAppFunction;
        _trackingIdAccessor = __trackingIdAccessor;
    }

    [HttpGet]
    [HasPermission(PermissionConstants.CustomerView)]
    public async Task<ActionResult<ApiResponse<PaginationResponse<CustomerResponse>>>> List([FromQuery] PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _customerAppFunction.ListAsync(__request, __cancellationToken);
        return Ok(ApiResponse<PaginationResponse<CustomerResponse>>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.CustomerView)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> GetById(Guid id, CancellationToken __cancellationToken)
    {
        var result = await _customerAppFunction.GetByIdAsync(id, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId));
    }

    [HttpPost]
    [HasPermission(PermissionConstants.CustomerCreate)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Create(CustomerRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _customerAppFunction.CreateAsync(__request, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Customer created."));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.CustomerUpdate)]
    public async Task<ActionResult<ApiResponse<CustomerResponse>>> Update(Guid id, CustomerRequest __request, CancellationToken __cancellationToken)
    {
        var result = await _customerAppFunction.UpdateAsync(id, __request, __cancellationToken);
        return Ok(ApiResponse<CustomerResponse>.Ok(result, _trackingIdAccessor.TrackingId, "Customer updated."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.CustomerDelete)]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id, CancellationToken __cancellationToken)
    {
        await _customerAppFunction.DeleteAsync(id, __cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, _trackingIdAccessor.TrackingId, "Customer deleted."));
    }
}
