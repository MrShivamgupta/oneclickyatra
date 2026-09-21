using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.UnitTests.AppFunctions;

public class CustomerPortalAppFunctionTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IBookingAppFunction> _bookingAppFunction = new();
    private readonly Mock<IPaymentAppFunction> _paymentAppFunction = new();
    private readonly Mock<IInvoiceAppFunction> _invoiceAppFunction = new();
    private readonly Mock<IQuotationAppFunction> _quotationAppFunction = new();
    private readonly Mock<IFeedbackRepository> _feedbackRepository = new();
    private readonly Mock<ICustomerDocumentRepository> _customerDocumentRepository = new();
    private readonly Mock<IFileStorageService> _fileStorageService = new();
    private readonly Mock<IVoucherPdfService> _voucherPdfService = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private CustomerPortalAppFunction CreateSut() => new(
        _customerRepository.Object,
        _userRepository.Object,
        _bookingAppFunction.Object,
        _paymentAppFunction.Object,
        _invoiceAppFunction.Object,
        _quotationAppFunction.Object,
        _feedbackRepository.Object,
        _customerDocumentRepository.Object,
        _fileStorageService.Object,
        _voucherPdfService.Object,
        _auditLogWriter.Object,
        Mock.Of<ILogger<CustomerPortalAppFunction>>());

    private CustomerModel StubExistingCustomer(Guid userId)
    {
        var customer = new CustomerModel { Id = Guid.NewGuid(), FullName = "Rahul Verma", Phone = "+919812300001", UserId = userId };
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        return customer;
    }

    [Fact]
    public async Task GetMyProfileAsync_NoLinkedCustomerYet_CreatesOneFromUser()
    {
        var userId = Guid.NewGuid();
        _customerRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((CustomerModel?)null);
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserModel { Id = userId, Email = "traveler@example.com", FullName = "Test Traveler" });

        var sut = CreateSut();
        var result = await sut.GetMyProfileAsync(userId, CancellationToken.None);

        Assert.Equal("traveler@example.com", result.Email);
        _customerRepository.Verify(r => r.CreateAsync(It.Is<CustomerModel>(c => c.UserId == userId && c.Email == "traveler@example.com"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyBookingByIdAsync_BookingBelongsToDifferentCustomer_ThrowsEntityNotFound()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = Guid.NewGuid() } });

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.GetMyBookingByIdAsync(userId, bookingId, CancellationToken.None));
    }

    [Fact]
    public async Task GetMyBookingByIdAsync_OwnBooking_ReturnsIt()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = customer.Id } });

        var sut = CreateSut();
        var result = await sut.GetMyBookingByIdAsync(userId, bookingId, CancellationToken.None);

        Assert.Equal(bookingId, result.Booking.Id);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_BookingNotCompleted_ThrowsBusiness()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = customer.Id, Status = "Confirmed" } });

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.SubmitFeedbackAsync(userId, bookingId, new FeedbackRequest { Rating = 5 }, CancellationToken.None));
        _feedbackRepository.Verify(r => r.CreateAsync(It.IsAny<FeedbackModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_AlreadySubmitted_ThrowsBusiness()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = customer.Id, Status = "Completed" } });
        _feedbackRepository.Setup(r => r.GetByBookingIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FeedbackModel { Id = Guid.NewGuid(), BookingId = bookingId });

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.SubmitFeedbackAsync(userId, bookingId, new FeedbackRequest { Rating = 4 }, CancellationToken.None));
    }

    [Fact]
    public async Task SubmitFeedbackAsync_CompletedAndNotYetReviewed_Succeeds()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, BookingNumber = "BK-1", CustomerId = customer.Id, Status = "Completed" } });
        _feedbackRepository.Setup(r => r.GetByBookingIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync((FeedbackModel?)null);

        var sut = CreateSut();
        var result = await sut.SubmitFeedbackAsync(userId, bookingId, new FeedbackRequest { Rating = 5, Comment = "Great trip!" }, CancellationToken.None);

        Assert.Equal(5, result.Rating);
        _feedbackRepository.Verify(r => r.CreateAsync(It.Is<FeedbackModel>(f => f.BookingId == bookingId && f.CustomerId == customer.Id && f.Rating == 5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVoucherPdfAsync_BookingNotYetConfirmed_ThrowsBusiness()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = customer.Id, Status = "PendingPayment" } });

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.GetVoucherPdfAsync(userId, bookingId, CancellationToken.None));
        _voucherPdfService.Verify(s => s.Generate(It.IsAny<BookingDetailResponse>()), Times.Never);
    }

    [Fact]
    public async Task GetVoucherPdfAsync_Confirmed_GeneratesPdf()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var bookingId = Guid.NewGuid();
        var detail = new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = customer.Id, Status = "Confirmed" } };
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync(detail);
        _voucherPdfService.Setup(s => s.Generate(detail)).Returns([1, 2, 3]);

        var sut = CreateSut();
        var result = await sut.GetVoucherPdfAsync(userId, bookingId, CancellationToken.None);

        Assert.Equal(new byte[] { 1, 2, 3 }, result);
    }

    [Fact]
    public async Task DownloadDocumentAsync_DocumentBelongsToDifferentCustomer_ThrowsEntityNotFound()
    {
        var userId = Guid.NewGuid();
        var customer = StubExistingCustomer(userId);
        var documentId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        _customerDocumentRepository.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerDocumentModel { Id = documentId, BookingId = bookingId, FileName = "x.pdf" });
        _bookingAppFunction.Setup(b => b.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingDetailResponse { Booking = new BookingResponse { Id = bookingId, CustomerId = Guid.NewGuid() } });

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DownloadDocumentAsync(userId, documentId, CancellationToken.None));
        _ = customer;
    }
}
