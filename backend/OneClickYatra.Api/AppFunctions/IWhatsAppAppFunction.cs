using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IWhatsAppAppFunction
{
    Task<PaginationResponse<WhatsAppTemplateResponse>> SearchTemplatesAsync(WhatsAppTemplateSearchRequest __request, CancellationToken __cancellationToken);
    Task<WhatsAppTemplateResponse> GetTemplateByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<WhatsAppTemplateResponse> CreateTemplateAsync(WhatsAppTemplateRequest __request, CancellationToken __cancellationToken);
    Task<WhatsAppTemplateResponse> UpdateTemplateAsync(Guid __id, WhatsAppTemplateRequest __request, CancellationToken __cancellationToken);
    Task DeleteTemplateAsync(Guid __id, CancellationToken __cancellationToken);

    Task<NotificationLogResponse> SendMessageAsync(WhatsAppSendMessageRequest __request, CancellationToken __cancellationToken);
    Task<PaginationResponse<NotificationLogResponse>> SearchNotificationLogsAsync(NotificationLogSearchRequest __request, CancellationToken __cancellationToken);

    Task ProcessWebhookAsync(string __rawBody, string? __signatureHeader, CancellationToken __cancellationToken);
    bool VerifyWebhookChallenge(string? __mode, string? __verifyToken);
}
