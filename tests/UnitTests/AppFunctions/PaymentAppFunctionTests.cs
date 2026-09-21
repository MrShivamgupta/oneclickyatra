using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Email;
using OneClickYatra.Api.Services.Payments;

namespace OneClickYatra.UnitTests.AppFunctions;

public class PaymentAppFunctionTests
{
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IRefundRepository> _refundRepository = new();
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IBookingAppFunction> _bookingAppFunction = new();
    private readonly Mock<IPaymentGateway> _paymentGateway = new();
    private readonly Mock<IBackgroundJobClient> _backgroundJobClient = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();
    private readonly Mock<IEmailNotificationSender> _emailNotificationSender = new();

    private PaymentAppFunction CreateSut() => new(
        _paymentRepository.Object,
        _refundRepository.Object,
        _bookingRepository.Object,
        _customerRepository.Object,
        _bookingAppFunction.Object,
        _paymentGateway.Object,
        _backgroundJobClient.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object,
        _emailNotificationSender.Object,
        Mock.Of<ILogger<PaymentAppFunction>>());

    private static BookingModel CreateBooking(string status = "PendingPayment", decimal total = 50000, decimal paid = 0) => new()
    {
        Id = Guid.NewGuid(),
        BookingNumber = "BK-20260101-ABCDEF",
        CustomerId = Guid.NewGuid(),
        Status = status,
        TotalAmount = total,
        AmountPaid = paid
    };

    [Fact]
    public async Task InitiateAsync_BookingNotPendingPayment_ThrowsBusiness()
    {
        var booking = CreateBooking(status: "Draft");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.InitiateAsync(new PaymentInitiateRequest { BookingId = booking.Id }, CancellationToken.None));
        _paymentGateway.Verify(g => g.CreateOrderAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitiateAsync_NoOutstandingBalance_ThrowsBusiness()
    {
        var booking = CreateBooking(status: "PendingPayment", total: 50000, paid: 50000);
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.InitiateAsync(new PaymentInitiateRequest { BookingId = booking.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task InitiateAsync_ValidBooking_CreatesOrderForRemainingBalance()
    {
        var booking = CreateBooking(status: "PendingPayment", total: 50000, paid: 20000);
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _paymentGateway.Setup(g => g.CreateOrderAsync(30000, "INR", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GatewayOrderResult("order_test123", "created"));
        _paymentGateway.SetupGet(g => g.PublicKeyId).Returns("rzp_test_key");

        var sut = CreateSut();
        var result = await sut.InitiateAsync(new PaymentInitiateRequest { BookingId = booking.Id }, CancellationToken.None);

        Assert.Equal(30000, result.Amount);
        Assert.Equal("order_test123", result.GatewayOrderId);
        Assert.Equal("rzp_test_key", result.GatewayKeyId);
        _paymentRepository.Verify(r => r.CreateAsync(It.Is<PaymentModel>(p => p.Amount == 30000 && p.GatewayOrderId == "order_test123"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_InvalidSignature_ThrowsInvalidWebhookSignature()
    {
        _paymentGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(false, null, null, null, null));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidWebhookSignatureException>(() => sut.ProcessWebhookAsync("{}", "bad-signature", null, CancellationToken.None));
        _paymentRepository.Verify(r => r.AddTransactionAsync(It.IsAny<PaymentTransactionModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_UnknownOrderId_DoesNothing()
    {
        _paymentGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(true, "payment.captured", "order_unknown", "pay_123", 500));
        _paymentRepository.Setup(r => r.GetByGatewayOrderIdAsync("order_unknown", It.IsAny<CancellationToken>())).ReturnsAsync((PaymentModel?)null);

        var sut = CreateSut();
        await sut.ProcessWebhookAsync("{}", "sig", null, CancellationToken.None);

        _paymentRepository.Verify(r => r.AddTransactionAsync(It.IsAny<PaymentTransactionModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_DuplicateEvent_DoesNotReprocess()
    {
        var payment = new PaymentModel { Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), Amount = 500, Status = "Pending" };
        _paymentGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(true, "payment.captured", "order_1", "pay_1", 500));
        _paymentRepository.Setup(r => r.GetByGatewayOrderIdAsync("order_1", It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _paymentRepository.Setup(r => r.AddTransactionAsync(It.IsAny<PaymentTransactionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var sut = CreateSut();
        await sut.ProcessWebhookAsync("{}", "sig", "evt_duplicate", CancellationToken.None);

        _paymentRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookingRepository.Verify(r => r.AddAmountPaidAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_AmountMismatch_ThrowsBusiness()
    {
        var payment = new PaymentModel { Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), Amount = 500, Status = "Pending" };
        _paymentGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(true, "payment.captured", "order_1", "pay_1", 999));
        _paymentRepository.Setup(r => r.GetByGatewayOrderIdAsync("order_1", It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _paymentRepository.Setup(r => r.AddTransactionAsync(It.IsAny<PaymentTransactionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.ProcessWebhookAsync("{}", "sig", "evt_1", CancellationToken.None));
        _paymentRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_CapturedAndFullyPaid_ConfirmsBooking()
    {
        var booking = CreateBooking(status: "PendingPayment", total: 500, paid: 0);
        var payment = new PaymentModel { Id = Guid.NewGuid(), BookingId = booking.Id, Amount = 500, Status = "Pending" };

        _paymentGateway.Setup(g => g.VerifyWebhookSignature(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new WebhookVerificationResult(true, "payment.captured", "order_1", "pay_1", 500));
        _paymentRepository.Setup(r => r.GetByGatewayOrderIdAsync("order_1", It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _paymentRepository.Setup(r => r.AddTransactionAsync(It.IsAny<PaymentTransactionModel>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _bookingRepository.SetupSequence(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingModel { Id = booking.Id, Status = "PendingPayment", TotalAmount = 500, AmountPaid = 500 });

        var sut = CreateSut();
        await sut.ProcessWebhookAsync("{}", "sig", "evt_1", CancellationToken.None);

        _paymentRepository.Verify(r => r.UpdateStatusAsync(payment.Id, "Paid", "pay_1", It.IsAny<CancellationToken>()), Times.Once);
        _bookingRepository.Verify(r => r.AddAmountPaidAsync(booking.Id, 500, It.IsAny<CancellationToken>()), Times.Once);
        _bookingAppFunction.Verify(b => b.UpdateStatusAsync(booking.Id, It.Is<BookingStatusRequest>(s => s.Status == "Confirmed"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiateRefundAsync_BookingNotRefundPending_ThrowsBusiness()
    {
        var booking = CreateBooking(status: "Confirmed");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.InitiateRefundAsync(new PaymentRefundRequest { BookingId = booking.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task InitiateRefundAsync_NoPaidPayment_ThrowsBusiness()
    {
        var booking = CreateBooking(status: "RefundPending");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _paymentRepository.Setup(r => r.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.InitiateRefundAsync(new PaymentRefundRequest { BookingId = booking.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task InitiateRefundAsync_InstantGatewayRefund_TransitionsBookingToRefunded()
    {
        var booking = CreateBooking(status: "RefundPending", total: 500, paid: 500);
        var payment = new PaymentModel { Id = Guid.NewGuid(), BookingId = booking.Id, Amount = 500, Status = "Paid", GatewayPaymentId = "pay_1" };
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _paymentRepository.Setup(r => r.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync([payment]);
        _paymentGateway.Setup(g => g.CreateRefundAsync("pay_1", 500, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GatewayRefundResult("rfnd_1", "processed"));

        var sut = CreateSut();
        var result = await sut.InitiateRefundAsync(new PaymentRefundRequest { BookingId = booking.Id, Reason = "Customer request" }, CancellationToken.None);

        Assert.Equal("Refunded", result.Status);
        _bookingAppFunction.Verify(b => b.UpdateStatusAsync(booking.Id, It.Is<BookingStatusRequest>(s => s.Status == "Refunded"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiateRefundAsync_AsyncGatewayRefund_LeavesBookingInRefundPending()
    {
        var booking = CreateBooking(status: "RefundPending", total: 500, paid: 500);
        var payment = new PaymentModel { Id = Guid.NewGuid(), BookingId = booking.Id, Amount = 500, Status = "Paid", GatewayPaymentId = "pay_1" };
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
        _paymentRepository.Setup(r => r.GetByBookingIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync([payment]);
        _paymentGateway.Setup(g => g.CreateRefundAsync("pay_1", 500, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GatewayRefundResult("rfnd_1", "processing"));

        var sut = CreateSut();
        var result = await sut.InitiateRefundAsync(new PaymentRefundRequest { BookingId = booking.Id }, CancellationToken.None);

        Assert.Equal("Processing", result.Status);
        _bookingAppFunction.Verify(b => b.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<BookingStatusRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
