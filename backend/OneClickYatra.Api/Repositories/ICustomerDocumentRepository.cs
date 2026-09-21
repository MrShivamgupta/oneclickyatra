using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public interface ICustomerDocumentRepository
{
    Task CreateAsync(CustomerDocumentModel __document, CancellationToken __cancellationToken);
    Task<CustomerDocumentModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<IReadOnlyList<CustomerDocumentModel>> GetByBookingIdAsync(Guid __bookingId, CancellationToken __cancellationToken);
}
