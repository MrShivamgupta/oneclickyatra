using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;

namespace OneClickYatra.Api.AppFunctions;

public interface IPaymentAppFunction
{
    Task<PaymentInitiateResponse> InitiateAsync(PaymentInitiateRequest __request, CancellationToken __cancellationToken);
    Task ProcessWebhookAsync(string __rawBody, string? __signatureHeader, string? __gatewayEventIdHeader, CancellationToken __cancellationToken);
    Task<RefundResponse> InitiateRefundAsync(PaymentRefundRequest __request, CancellationToken __cancellationToken);
    Task<PaginationResponse<PaymentResponse>> SearchAsync(PaymentSearchRequest __request, CancellationToken __cancellationToken);
    Task<PaymentResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
}
