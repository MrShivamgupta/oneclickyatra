using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface ICustomerAppFunction
{
    Task<PaginationResponse<CustomerResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<CustomerResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CustomerResponse> CreateAsync(CustomerRequest __request, CancellationToken __cancellationToken);
    Task<CustomerResponse> UpdateAsync(Guid __id, CustomerRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
