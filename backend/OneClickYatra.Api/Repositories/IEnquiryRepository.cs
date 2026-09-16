using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IEnquiryRepository
{
    Task<EnquiryModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<PaginationResponse<EnquiryModel>> SearchAsync(EnquirySearchRequest __request, CancellationToken __cancellationToken);
    Task<Guid> CreateAsync(EnquiryModel __enquiry, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
