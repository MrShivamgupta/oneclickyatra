using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IWhatsAppTemplateRepository
{
    Task<WhatsAppTemplateModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<WhatsAppTemplateModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken);
    Task<PaginationResponse<WhatsAppTemplateModel>> SearchAsync(WhatsAppTemplateSearchRequest __request, CancellationToken __cancellationToken);
    Task CreateAsync(WhatsAppTemplateModel __template, CancellationToken __cancellationToken);
    Task UpdateAsync(WhatsAppTemplateModel __template, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
