using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface ILeadRepository
{
    Task<LeadModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PaginationResponse<LeadModel>> SearchAsync(LeadSearchRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(LeadModel __lead, CancellationToken __cancellationToken);
    Task UpdateAsync(LeadModel __lead, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task AssignAsync(Guid __id, Guid __assignedToUserId, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task UpdateScoreAsync(Guid __id, int __leadScore, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task LinkCustomerAsync(Guid __id, Guid __customerId, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
