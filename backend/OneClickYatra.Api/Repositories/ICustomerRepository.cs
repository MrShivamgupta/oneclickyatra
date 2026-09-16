using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface ICustomerRepository
{
    Task<CustomerModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<CustomerModel?> GetByPhoneAsync(string __phone, CancellationToken __cancellationToken);
    Task<PaginationResponse<CustomerModel>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(CustomerModel __customer, CancellationToken __cancellationToken);
    Task UpdateAsync(CustomerModel __customer, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
