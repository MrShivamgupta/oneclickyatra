using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IEmailTemplateAppFunction
{
    Task<PaginationResponse<EmailTemplateResponse>> SearchAsync(EmailTemplateSearchRequest __request, CancellationToken __cancellationToken);
    Task<EmailTemplateResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<EmailTemplateResponse> CreateAsync(UpsertEmailTemplateRequest __request, CancellationToken __cancellationToken);
    Task<EmailTemplateResponse> UpdateAsync(Guid __id, UpsertEmailTemplateRequest __request, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, CancellationToken __cancellationToken);
}
