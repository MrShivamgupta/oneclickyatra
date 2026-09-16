using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.UnitTests.AppFunctions;

public class BookingAppFunctionTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<ILeadRepository> _leadRepository = new();
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<IPackageRepository> _packageRepository = new();
    private readonly Mock<IQuotationRepository> _quotationRepository = new();
    private readonly Mock<ILeadAppFunction> _leadAppFunction = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<ITrackingIdAccessor> _trackingIdAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private BookingAppFunction CreateSut() => new(
        _bookingRepository.Object,
        _customerRepository.Object,
        _leadRepository.Object,
        _destinationRepository.Object,
        _packageRepository.Object,
        _quotationRepository.Object,
        _leadAppFunction.Object,
        _currentUserAccessor.Object,
        _trackingIdAccessor.Object,
        _auditLogWriter.Object);

    private static BookingModel CreateBooking(string status) => new()
    {
        Id = Guid.NewGuid(),
        BookingNumber = "BK-20260101-ABCDEF",
        CustomerId = Guid.NewGuid(),
        Status = status
    };

    [Theory]
    [InlineData("Draft", "Quoted")]
    [InlineData("Draft", "Cancelled")]
    [InlineData("Quoted", "PendingPayment")]
    [InlineData("PendingPayment", "Confirmed")]
    [InlineData("Confirmed", "InProgress")]
    [InlineData("Confirmed", "RefundPending")]
    [InlineData("InProgress", "Completed")]
    [InlineData("Cancelled", "RefundPending")]
    [InlineData("RefundPending", "Refunded")]
    public async Task UpdateStatusAsync_AllowedTransition_Succeeds(string from, string to)
    {
        var booking = CreateBooking(from);
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();
        var result = await sut.UpdateStatusAsync(booking.Id, new BookingStatusRequest { Status = to }, CancellationToken.None);

        Assert.Equal(to, result.Status);
        _bookingRepository.Verify(r => r.UpdateStatusAsync(booking.Id, to, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
        _bookingRepository.Verify(r => r.AddStatusHistoryAsync(
            It.Is<BookingStatusHistoryModel>(h => h.OldStatus == from && h.NewStatus == to), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Draft", "Confirmed")]
    [InlineData("Draft", "InProgress")]
    [InlineData("Quoted", "Confirmed")]
    [InlineData("Confirmed", "Draft")]
    [InlineData("InProgress", "Cancelled")]
    [InlineData("Completed", "Cancelled")]
    [InlineData("Refunded", "Confirmed")]
    [InlineData("Cancelled", "Confirmed")]
    public async Task UpdateStatusAsync_DisallowedTransition_ThrowsBusiness(string from, string to)
    {
        var booking = CreateBooking(from);
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.UpdateStatusAsync(booking.Id, new BookingStatusRequest { Status = to }, CancellationToken.None));
        _bookingRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_WritesCancellationReasonAndHistory()
    {
        var booking = CreateBooking("PendingPayment");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();
        var result = await sut.CancelAsync(booking.Id, new BookingCancelRequest { Reason = "Customer changed plans." }, CancellationToken.None);

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal("Customer changed plans.", result.CancellationReason);
        _bookingRepository.Verify(r => r.SetCancellationReasonAsync(booking.Id, "Customer changed plans.", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiateRefundAsync_FromCompleted_ThrowsBusiness()
    {
        var booking = CreateBooking("Completed");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.InitiateRefundAsync(booking.Id, new BookingRefundRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_StatusNotDraft_ThrowsBusiness()
    {
        var booking = CreateBooking("Quoted");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.DeleteAsync(booking.Id, CancellationToken.None));
        _bookingRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_StatusDraft_Succeeds()
    {
        var booking = CreateBooking("Draft");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();
        await sut.DeleteAsync(booking.Id, CancellationToken.None);

        _bookingRepository.Verify(r => r.DeleteAsync(booking.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReplacePassengersAsync_CancelledBooking_ThrowsBusiness()
    {
        var booking = CreateBooking("Cancelled");
        _bookingRepository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var sut = CreateSut();
        var passengers = new List<BookingPassengerRequest> { new() { FullName = "Rahul Verma", IsLeadPassenger = true } };

        await Assert.ThrowsAsync<BusinessException>(() => sut.ReplacePassengersAsync(booking.Id, passengers, CancellationToken.None));
        _bookingRepository.Verify(r => r.ReplacePassengersAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<BookingPassengerModel>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UnknownCustomerId_ThrowsEntityNotFound()
    {
        var customerId = Guid.NewGuid();
        _customerRepository.Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync((CustomerModel?)null);

        var sut = CreateSut();
        var request = new BookingRequest { CustomerId = customerId };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.CreateAsync(request, CancellationToken.None));
        _bookingRepository.Verify(r => r.CreateAsync(It.IsAny<BookingModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConvertFromQuotationAsync_QuotationNotApproved_ThrowsBusiness()
    {
        var quotation = new QuotationModel { Id = Guid.NewGuid(), Status = "Sent", LeadId = Guid.NewGuid() };
        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.ConvertFromQuotationAsync(quotation.Id, CancellationToken.None));
        _bookingRepository.Verify(r => r.CreateAsync(It.IsAny<BookingModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConvertFromQuotationAsync_ApprovedWithoutExistingCustomer_ConvertsLeadFirst()
    {
        var leadId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var quotation = new QuotationModel
        {
            Id = Guid.NewGuid(),
            QuotationNumber = "QT-20260101-ABCDEF",
            Status = "Approved",
            LeadId = leadId,
            CustomerId = null,
            SelectedOptionId = optionId
        };
        var option = new QuotationOptionModel { Id = optionId, QuotationId = quotation.Id, OptionName = "Standard", NumberOfPeople = 2, TotalPrice = 30000 };
        var newCustomer = new OneClickYatra.Api.Models.Responses.CustomerResponse { Id = Guid.NewGuid(), FullName = "Rahul Verma", Phone = "+919812300001" };

        _quotationRepository.Setup(r => r.GetByIdAsync(quotation.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quotation);
        _quotationRepository.Setup(r => r.GetOptionByIdAsync(optionId, It.IsAny<CancellationToken>())).ReturnsAsync(option);
        _leadAppFunction.Setup(f => f.ConvertToCustomerAsync(leadId, It.IsAny<CancellationToken>())).ReturnsAsync(newCustomer);

        var sut = CreateSut();
        var result = await sut.ConvertFromQuotationAsync(quotation.Id, CancellationToken.None);

        Assert.Equal("Quoted", result.Status);
        Assert.Equal(newCustomer.Id, result.CustomerId);
        _leadAppFunction.Verify(f => f.ConvertToCustomerAsync(leadId, It.IsAny<CancellationToken>()), Times.Once);
        _quotationRepository.Verify(r => r.UpdateStatusAsync(quotation.Id, "Converted", It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
