using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IEmailTemplateRepository
{
    Task<EmailTemplateModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<EmailTemplateModel?> GetByNameAsync(string __name, CancellationToken __cancellationToken);
    Task<PaginationResponse<EmailTemplateModel>> SearchAsync(EmailTemplateSearchRequest __request, CancellationToken __cancellationToken);
    Task CreateAsync(EmailTemplateModel __template, CancellationToken __cancellationToken);
    Task UpdateAsync(EmailTemplateModel __template, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
}
