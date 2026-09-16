using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface ILeadAppFunction
{
    Task<PaginationResponse<LeadResponse>> SearchAsync(LeadSearchRequest __request, CancellationToken __cancellationToken);
    Task<LeadResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<LeadResponse> CreateAsync(LeadRequest __request, CancellationToken __cancellationToken);
    Task<LeadResponse> UpdateAsync(Guid __id, LeadRequest __request, CancellationToken __cancellationToken);
    Task<LeadResponse> UpdateStatusAsync(Guid __id, LeadStatusRequest __request, CancellationToken __cancellationToken);
    Task<LeadResponse> AssignAsync(Guid __id, LeadAssignRequest __request, CancellationToken __cancellationToken);
    Task<LeadResponse> UpdateScoreAsync(Guid __id, LeadScoreRequest __request, CancellationToken __cancellationToken);
    Task<CustomerResponse> ConvertToCustomerAsync(Guid __id, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
