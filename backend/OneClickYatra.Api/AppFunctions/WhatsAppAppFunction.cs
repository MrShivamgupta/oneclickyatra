using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.WhatsApp;

namespace OneClickYatra.Api.AppFunctions;

public sealed class WhatsAppAppFunction : IWhatsAppAppFunction
{
    private readonly IWhatsAppTemplateRepository _templateRepository;
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly IWhatsAppGateway _whatsAppGateway;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<WhatsAppAppFunction> _logger;

    public WhatsAppAppFunction(
        IWhatsAppTemplateRepository __templateRepository,
        INotificationLogRepository __notificationLogRepository,
        IWhatsAppGateway __whatsAppGateway,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ILogger<WhatsAppAppFunction> __logger)
    {
        _templateRepository = __templateRepository;
        _notificationLogRepository = __notificationLogRepository;
        _whatsAppGateway = __whatsAppGateway;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<PaginationResponse<WhatsAppTemplateResponse>> SearchTemplatesAsync(WhatsAppTemplateSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _templateRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<WhatsAppTemplateResponse>.Create(page.Items.Select(ToTemplateResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<WhatsAppTemplateResponse> GetTemplateByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("WhatsAppTemplate", __id);
        return ToTemplateResponse(template);
    }

    public async Task<WhatsAppTemplateResponse> CreateTemplateAsync(WhatsAppTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var existing = await _templateRepository.GetByNameAsync(__request.Name, __cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException($"A WhatsApp template named '{__request.Name}' already exists.");
        }

        var template = new WhatsAppTemplateModel
        {
            Id = Guid.NewGuid(),
            Name = __request.Name,
            Category = __request.Category,
            BodyText = __request.BodyText,
            IsActive = __request.IsActive,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _templateRepository.CreateAsync(template, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "whatsapp.template.created", "WhatsAppTemplate", template.Id.ToString(), null, template.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "whatsapp.template.created", "WhatsAppTemplate", template.Id);
        }

        return ToTemplateResponse(template);
    }

    public async Task<WhatsAppTemplateResponse> UpdateTemplateAsync(Guid __id, WhatsAppTemplateRequest __request, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("WhatsAppTemplate", __id);

        var existingWithName = await _templateRepository.GetByNameAsync(__request.Name, __cancellationToken);
        if (existingWithName is not null && existingWithName.Id != __id)
        {
            throw new BusinessException($"A WhatsApp template named '{__request.Name}' already exists.");
        }

        var oldName = template.Name;
        template.Name = __request.Name;
        template.Category = __request.Category;
        template.BodyText = __request.BodyText;
        template.IsActive = __request.IsActive;
        template.UpdatedBy = _currentUserAccessor.UserId;

        await _templateRepository.UpdateAsync(template, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "whatsapp.template.updated", "WhatsAppTemplate", template.Id.ToString(), oldName, template.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "whatsapp.template.updated", "WhatsAppTemplate", template.Id);
        }

        return ToTemplateResponse(template);
    }

    public async Task DeleteTemplateAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("WhatsAppTemplate", __id);

        await _templateRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "whatsapp.template.deleted", "WhatsAppTemplate", __id.ToString(), template.Name, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "whatsapp.template.deleted", "WhatsAppTemplate", __id);
        }
    }

    /// <summary>Looks up the named template, calls the gateway, and always logs the attempt — Sent
    /// with the gateway's message id on success, or Failed with the error message when the gateway
    /// call itself fails (a bad phone number, an unapproved template, etc.). A missing/absent
    /// configuration is treated differently: it is an environment problem, not a per-message
    /// failure, so WhatsAppNotConfiguredException is left to bubble up as a 503 rather than being
    /// logged as a failed send.</summary>
    public async Task<NotificationLogResponse> SendMessageAsync(WhatsAppSendMessageRequest __request, CancellationToken __cancellationToken)
    {
        var template = await _templateRepository.GetByNameAsync(__request.TemplateName, __cancellationToken)
            ?? throw new EntityNotFoundException("WhatsAppTemplate", __request.TemplateName);
        if (!template.IsActive)
        {
            throw new BusinessException($"Template '{template.Name}' is not active.");
        }

        var log = new NotificationLogModel
        {
            Id = Guid.NewGuid(),
            Channel = "WhatsApp",
            RecipientPhone = __request.PhoneNumber,
            TemplateId = template.Id,
            Subject = template.Name,
            Body = ApplyParameters(template.BodyText, __request.Parameters),
            CreatedBy = _currentUserAccessor.UserId
        };

        try
        {
            var gatewayMessageId = await _whatsAppGateway.SendTemplateMessageAsync(__request.PhoneNumber, __request.TemplateName, __request.Parameters, __cancellationToken);
            log.Status = "Sent";
            log.GatewayMessageId = gatewayMessageId;
            log.SentAt = DateTime.UtcNow;
        }
        catch (WhatsAppGatewayException exception)
        {
            log.Status = "Failed";
            log.ErrorMessage = exception.Message;
            _logger.LogWarning(exception, "WhatsApp send to template {TemplateName} failed for {PhoneNumber}", __request.TemplateName, __request.PhoneNumber);
        }

        await _notificationLogRepository.CreateAsync(log, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "whatsapp.message.sent", "NotificationLog", log.Id.ToString(), null, log.Status, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "whatsapp.message.sent", "NotificationLog", log.Id);
        }

        // CreateAsync's INSERT stamps CreatedAt via SYSUTCDATETIME() rather than the (unset) model
        // property — set it here purely so the response returned to the caller reflects it, the
        // same approach RefundResponse.CreatedAt already uses in PaymentAppFunction.
        log.CreatedAt = DateTime.UtcNow;
        log.TemplateName = template.Name;
        return ToLogResponse(log);
    }

    public async Task<PaginationResponse<NotificationLogResponse>> SearchNotificationLogsAsync(NotificationLogSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _notificationLogRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<NotificationLogResponse>.Create(page.Items.Select(ToLogResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    /// <summary>Verifies the webhook signature and logs the inbound message as Received. Generating
    /// and sending an automated reply back to the customer (the "automated replies" capability from
    /// the SRS) is intentionally out of scope for this pass — this is only the logging hook point a
    /// future auto-responder job would read from.</summary>
    public async Task ProcessWebhookAsync(string __rawBody, string? __signatureHeader, CancellationToken __cancellationToken)
    {
        var verification = _whatsAppGateway.VerifyWebhookSignature(__rawBody, __signatureHeader);
        if (!verification.IsValid)
        {
            throw new InvalidWebhookSignatureException();
        }

        if (string.IsNullOrEmpty(verification.MessageId))
        {
            // Not an inbound user message (e.g. a delivery/read status callback) — acknowledge and ignore.
            return;
        }

        var log = new NotificationLogModel
        {
            Id = Guid.NewGuid(),
            Channel = "WhatsApp",
            RecipientPhone = verification.FromPhoneNumber,
            Body = verification.MessageBody,
            Status = "Received",
            GatewayMessageId = verification.MessageId
        };

        // CreateAsync returns false (no-op) if this exact GatewayMessageId was already logged —
        // Meta retries webhook deliveries that are not acknowledged quickly enough.
        var wasNewlyLogged = await _notificationLogRepository.CreateAsync(log, __cancellationToken);
        if (wasNewlyLogged)
        {
            _logger.LogInformation("Received WhatsApp inbound message {GatewayMessageId} from {PhoneNumber}", verification.MessageId, verification.FromPhoneNumber);
        }
        else
        {
            _logger.LogWarning("Ignored duplicate WhatsApp webhook delivery for {GatewayMessageId} from {PhoneNumber}", verification.MessageId, verification.FromPhoneNumber);
        }
    }

    public bool VerifyWebhookChallenge(string? __mode, string? __verifyToken) => _whatsAppGateway.VerifyWebhookChallenge(__mode, __verifyToken);

    /// <summary>Replaces Meta's {{1}}, {{2}}, ... template placeholders with the supplied parameter
    /// values, purely so NotificationLogs.Body reads as the actual message an admin can review —
    /// the parameters are also sent to Meta as-is via SendTemplateMessageAsync.</summary>
    private static string ApplyParameters(string __bodyText, string[] __parameters)
    {
        var body = __bodyText;
        for (var index = 0; index < __parameters.Length; index++)
        {
            body = body.Replace($"{{{{{index + 1}}}}}", __parameters[index]);
        }

        return body;
    }

    private static WhatsAppTemplateResponse ToTemplateResponse(WhatsAppTemplateModel template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Category = template.Category,
        BodyText = template.BodyText,
        IsActive = template.IsActive,
        CreatedAt = template.CreatedAt
    };

    private static NotificationLogResponse ToLogResponse(NotificationLogModel log) => new()
    {
        Id = log.Id,
        Channel = log.Channel,
        RecipientPhone = log.RecipientPhone,
        RecipientEmail = log.RecipientEmail,
        TemplateId = log.TemplateId,
        TemplateName = log.TemplateName,
        Subject = log.Subject,
        Body = log.Body,
        Status = log.Status,
        GatewayMessageId = log.GatewayMessageId,
        ErrorMessage = log.ErrorMessage,
        SentAt = log.SentAt,
        CreatedAt = log.CreatedAt
    };
}
