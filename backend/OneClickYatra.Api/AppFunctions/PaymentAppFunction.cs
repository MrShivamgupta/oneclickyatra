using System.Security.Cryptography;
using System.Text;
using Hangfire;
using OneClickYatra.Api.BackgroundJobs;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Email;
using OneClickYatra.Api.Services.Payments;

namespace OneClickYatra.Api.AppFunctions;

public sealed class PaymentAppFunction : IPaymentAppFunction
{
    /// <summary>See BookingAppFunction's identical constant for why this is a shared placeholder
    /// literal rather than sourced from an agency-settings table (no such domain exists yet).</summary>
    private const string SupportPhonePlaceholder = "+91-11-4567-8900";

    private readonly IPaymentRepository _paymentRepository;
    private readonly IRefundRepository _refundRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IBookingAppFunction _bookingAppFunction;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly IEmailNotificationSender _emailNotificationSender;
    private readonly ILogger<PaymentAppFunction> _logger;

    public PaymentAppFunction(
        IPaymentRepository __paymentRepository,
        IRefundRepository __refundRepository,
        IBookingRepository __bookingRepository,
        ICustomerRepository __customerRepository,
        IBookingAppFunction __bookingAppFunction,
        IPaymentGateway __paymentGateway,
        IBackgroundJobClient __backgroundJobClient,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        IEmailNotificationSender __emailNotificationSender,
        ILogger<PaymentAppFunction> __logger)
    {
        _paymentRepository = __paymentRepository;
        _refundRepository = __refundRepository;
        _bookingRepository = __bookingRepository;
        _customerRepository = __customerRepository;
        _bookingAppFunction = __bookingAppFunction;
        _paymentGateway = __paymentGateway;
        _backgroundJobClient = __backgroundJobClient;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _emailNotificationSender = __emailNotificationSender;
        _logger = __logger;
    }

    /// <summary>Best-effort — the caller's own operation (payment captured / refund confirmed) has
    /// already committed by the time this runs, so a failed email must never mask that.</summary>
    private async Task SendPaymentEmailAsync(Guid __customerId, string __templateName, Dictionary<string, string> __placeholders, Guid __contextEntityId, CancellationToken __cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(__customerId, __cancellationToken);
            if (string.IsNullOrWhiteSpace(customer?.Email))
            {
                _logger.LogWarning("Skipped {TemplateName} email for {EntityId}: no email address on file for Customer {CustomerId}", __templateName, __contextEntityId, __customerId);
                return;
            }

            await _emailNotificationSender.SendAsync(__templateName, customer.Email, __placeholders, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to send {TemplateName} email for {EntityId}", __templateName, __contextEntityId);
        }
    }

    public async Task<PaymentInitiateResponse> InitiateAsync(PaymentInitiateRequest __request, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__request.BookingId, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __request.BookingId);
        if (booking.Status != "PendingPayment")
        {
            throw new BusinessException("Only a booking awaiting payment can have a payment initiated.");
        }

        var remainingAmount = booking.TotalAmount - booking.AmountPaid;
        if (remainingAmount <= 0)
        {
            throw new BusinessException("This booking has no outstanding balance.");
        }

        var payment = new PaymentModel
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Amount = remainingAmount,
            Currency = "INR",
            Status = "Created",
            GatewayProvider = "Razorpay",
            CreatedBy = _currentUserAccessor.UserId
        };

        var receiptId = $"{booking.BookingNumber}-{payment.Id.ToString("N")[..8]}";
        var orderResult = await _paymentGateway.CreateOrderAsync(remainingAmount, payment.Currency, receiptId, __cancellationToken);

        payment.GatewayOrderId = orderResult.GatewayOrderId;
        payment.Status = "Pending";

        await _paymentRepository.CreateAsync(payment, __cancellationToken);
        await _paymentRepository.AddTransactionAsync(new PaymentTransactionModel
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            EventType = "OrderCreated"
        }, __cancellationToken);

        _logger.LogInformation(
            "Payment initiated. PaymentId: {PaymentId}, BookingId: {BookingId}, Amount: {Amount}, GatewayOrderId: {GatewayOrderId}, UserId: {UserId}",
            payment.Id, booking.Id, remainingAmount, orderResult.GatewayOrderId, _currentUserAccessor.UserId);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "payment.initiated", "Payment", payment.Id.ToString(), null, orderResult.GatewayOrderId, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "payment.initiated", "Payment", payment.Id);
        }

        return new PaymentInitiateResponse
        {
            PaymentId = payment.Id,
            GatewayOrderId = orderResult.GatewayOrderId,
            Amount = remainingAmount,
            Currency = payment.Currency,
            GatewayKeyId = _paymentGateway.PublicKeyId
        };
    }

    public async Task ProcessWebhookAsync(string __rawBody, string? __signatureHeader, string? __gatewayEventIdHeader, CancellationToken __cancellationToken)
    {
        var verification = _paymentGateway.VerifyWebhookSignature(__rawBody, __signatureHeader);
        if (!verification.IsValid)
        {
            throw new InvalidWebhookSignatureException();
        }

        if (string.IsNullOrEmpty(verification.GatewayOrderId))
        {
            // Not a payment-order event we track (e.g. a different event type) — acknowledge and ignore.
            _logger.LogWarning("Received webhook with an untracked event type and no GatewayOrderId. EventType: {EventType}", verification.EventType);
            return;
        }

        var payment = await _paymentRepository.GetByGatewayOrderIdAsync(verification.GatewayOrderId, __cancellationToken);
        if (payment is null)
        {
            // Unknown order id — nothing on our side references it; acknowledge without processing.
            _logger.LogWarning("Received webhook for an unknown GatewayOrderId {GatewayOrderId}. EventType: {EventType}", verification.GatewayOrderId, verification.EventType);
            return;
        }

        var idempotencyKey = __gatewayEventIdHeader ?? ComputeFallbackIdempotencyKey(__rawBody);
        var isNewEvent = await _paymentRepository.AddTransactionAsync(new PaymentTransactionModel
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            EventType = verification.EventType ?? "Unknown",
            GatewayEventId = idempotencyKey,
            RawPayload = __rawBody
        }, __cancellationToken);

        if (!isNewEvent)
        {
            // Already processed this exact gateway event — idempotent no-op, do not reprocess.
            _logger.LogWarning(
                "Duplicate webhook event received and skipped. PaymentId: {PaymentId}, GatewayOrderId: {GatewayOrderId}, GatewayEventId: {GatewayEventId}",
                payment.Id, verification.GatewayOrderId, idempotencyKey);
            return;
        }

        if (verification.AmountInRupees.HasValue && verification.AmountInRupees.Value != payment.Amount)
        {
            _logger.LogWarning(
                "Webhook amount mismatch for {PaymentId}. ExpectedAmount: {ExpectedAmount}, ActualAmount: {ActualAmount}",
                payment.Id, payment.Amount, verification.AmountInRupees.Value);

            try
            {
                await _auditLogWriter.LogAsync(null, "payment.amount_mismatch", "Payment", payment.Id.ToString(), payment.Amount.ToString(), verification.AmountInRupees.Value.ToString(), __cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "payment.amount_mismatch", "Payment", payment.Id);
            }

            throw new BusinessException("Webhook amount did not match the expected payment amount.");
        }

        if (verification.EventType == "payment.captured")
        {
            await _paymentRepository.UpdateStatusAsync(payment.Id, "Paid", verification.GatewayPaymentId, __cancellationToken);
            await _bookingRepository.AddAmountPaidAsync(payment.BookingId, payment.Amount, __cancellationToken);

            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId, __cancellationToken);
            if (booking is not null && booking.Status == "PendingPayment" && booking.AmountPaid >= booking.TotalAmount)
            {
                await _bookingAppFunction.UpdateStatusAsync(booking.Id, new BookingStatusRequest { Status = "Confirmed", Reason = "Payment received in full." }, __cancellationToken);
                _backgroundJobClient.Enqueue<GenerateInvoiceJob>(job => job.RunAsync(booking.Id, CancellationToken.None));
            }

            _logger.LogInformation(
                "Payment captured. PaymentId: {PaymentId}, BookingId: {BookingId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, Amount: {Amount}",
                payment.Id, payment.BookingId, "Pending", "Paid", payment.Amount);

            try
            {
                await _auditLogWriter.LogAsync(null, "payment.captured", "Payment", payment.Id.ToString(), "Pending", "Paid", __cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "payment.captured", "Payment", payment.Id);
            }

            if (booking is not null)
            {
                var balanceDue = Math.Max(0, booking.TotalAmount - booking.AmountPaid);
                await SendPaymentEmailAsync(booking.CustomerId, "PaymentReceipt", new Dictionary<string, string>
                {
                    ["BookingReference"] = booking.BookingNumber,
                    ["CustomerName"] = booking.CustomerName ?? "Customer",
                    ["ReceiptNumber"] = payment.Id.ToString("N")[..12].ToUpperInvariant(),
                    ["AmountPaid"] = "Rs. " + payment.Amount.ToString("N2"),
                    ["PaymentDate"] = DateTime.UtcNow.ToString("dd MMM yyyy"),
                    ["PaymentMethod"] = payment.GatewayProvider,
                    ["BalanceDue"] = "Rs. " + balanceDue.ToString("N2")
                }, payment.Id, __cancellationToken);
            }
        }
        else if (verification.EventType == "payment.failed")
        {
            await _paymentRepository.UpdateStatusAsync(payment.Id, "Failed", verification.GatewayPaymentId, __cancellationToken);

            _logger.LogInformation(
                "Payment failed. PaymentId: {PaymentId}, BookingId: {BookingId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, Amount: {Amount}",
                payment.Id, payment.BookingId, "Pending", "Failed", payment.Amount);

            try
            {
                await _auditLogWriter.LogAsync(null, "payment.failed", "Payment", payment.Id.ToString(), "Pending", "Failed", __cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "payment.failed", "Payment", payment.Id);
            }
        }
    }

    public async Task<RefundResponse> InitiateRefundAsync(PaymentRefundRequest __request, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__request.BookingId, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __request.BookingId);
        if (booking.Status != "RefundPending")
        {
            throw new BusinessException("Only a booking in RefundPending status can be refunded through the gateway.");
        }

        var payments = await _paymentRepository.GetByBookingIdAsync(booking.Id, __cancellationToken);
        var paidPayment = payments.FirstOrDefault(p => p.Status == "Paid")
            ?? throw new BusinessException("No captured payment was found for this booking to refund.");
        if (string.IsNullOrEmpty(paidPayment.GatewayPaymentId))
        {
            throw new BusinessException("This payment has no gateway payment id recorded and cannot be refunded automatically.");
        }

        var refund = new RefundModel
        {
            Id = Guid.NewGuid(),
            PaymentId = paidPayment.Id,
            BookingId = booking.Id,
            Amount = paidPayment.Amount,
            Reason = __request.Reason,
            Status = "Requested",
            RequestedBy = _currentUserAccessor.UserId
        };
        await _refundRepository.CreateAsync(refund, __cancellationToken);

        _logger.LogInformation(
            "Refund initiated. PaymentId: {PaymentId}, BookingId: {BookingId}, RefundAmount: {RefundAmount}, Reason: {Reason}, UserId: {UserId}",
            paidPayment.Id, booking.Id, refund.Amount, __request.Reason, _currentUserAccessor.UserId);

        var gatewayResult = await _paymentGateway.CreateRefundAsync(paidPayment.GatewayPaymentId, refund.Amount, __request.Reason, __cancellationToken);

        // Razorpay refunds for card/UPI are typically instant ("processed"); netbanking refunds can
        // take a few days ("processing"). Only the instant case is auto-completed here — an async
        // refund is left as Processing/RefundPending for an admin to confirm once the gateway settles
        // it (via the existing PUT /bookings/{id}/status, RefundPending -> Refunded is already a
        // valid transition), rather than building a second webhook handler for refund events in this pass.
        var isInstantlyProcessed = gatewayResult.Status == "processed";
        var refundStatus = isInstantlyProcessed ? "Refunded" : "Processing";
        await _refundRepository.UpdateStatusAsync(refund.Id, refundStatus, gatewayResult.GatewayRefundId, _currentUserAccessor.UserId, __cancellationToken);

        if (isInstantlyProcessed)
        {
            await _paymentRepository.UpdateStatusAsync(paidPayment.Id, "Refunded", null, __cancellationToken);
            await _bookingAppFunction.UpdateStatusAsync(booking.Id, new BookingStatusRequest { Status = "Refunded", Reason = __request.Reason }, __cancellationToken);
        }

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "payment.refund_initiated", "Booking", booking.Id.ToString(), "RefundPending", refundStatus, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "payment.refund_initiated", "Booking", booking.Id);
        }

        // Only send the confirmation once the refund is actually confirmed (Razorpay's instant
        // card/UPI path) — a "Processing" netbanking refund isn't confirmed yet, so no email here;
        // an admin later completing it via the booking-status endpoint is out of this method's scope.
        if (isInstantlyProcessed)
        {
            await SendPaymentEmailAsync(booking.CustomerId, "RefundConfirmation", new Dictionary<string, string>
            {
                ["BookingReference"] = booking.BookingNumber,
                ["CustomerName"] = booking.CustomerName ?? "Customer",
                ["RefundAmount"] = "Rs. " + refund.Amount.ToString("N2"),
                ["RefundDate"] = DateTime.UtcNow.ToString("dd MMM yyyy"),
                ["RefundMode"] = "your original payment method",
                ["SupportPhone"] = SupportPhonePlaceholder
            }, refund.Id, __cancellationToken);
        }

        return new RefundResponse
        {
            Id = refund.Id,
            PaymentId = refund.PaymentId,
            BookingId = refund.BookingId,
            Amount = refund.Amount,
            Reason = refund.Reason,
            Status = refundStatus,
            GatewayRefundId = gatewayResult.GatewayRefundId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<PaginationResponse<PaymentResponse>> SearchAsync(PaymentSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _paymentRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<PaymentResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<PaymentResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Payment", __id);
        return ToResponse(payment);
    }

    private static string ComputeFallbackIdempotencyKey(string __rawBody)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(__rawBody));
        return Convert.ToHexStringLower(hash);
    }

    private static PaymentResponse ToResponse(PaymentModel payment) => new()
    {
        Id = payment.Id,
        BookingId = payment.BookingId,
        BookingNumber = payment.BookingNumber,
        CustomerName = payment.CustomerName,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        GatewayProvider = payment.GatewayProvider,
        GatewayOrderId = payment.GatewayOrderId,
        GatewayPaymentId = payment.GatewayPaymentId,
        CreatedAt = payment.CreatedAt
    };
}
