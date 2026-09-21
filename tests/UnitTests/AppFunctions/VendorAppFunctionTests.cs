using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.UnitTests.AppFunctions;

public class VendorAppFunctionTests
{
    private readonly Mock<IVendorRepository> _vendorRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IDestinationRepository> _destinationRepository = new();
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<IVendorInvoiceRepository> _vendorInvoiceRepository = new();
    private readonly Mock<IFileStorageService> _fileStorageService = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private VendorAppFunction CreateSut() => new(
        _vendorRepository.Object,
        _userRepository.Object,
        _destinationRepository.Object,
        _bookingRepository.Object,
        _vendorInvoiceRepository.Object,
        _fileStorageService.Object,
        _currentUserAccessor.Object,
        _auditLogWriter.Object,
        Mock.Of<ILogger<VendorAppFunction>>());

    private static VendorModel CreateVendor() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Blue Lagoon Resorts",
        VendorType = "Hotel",
        Email = "ops@bluelagoon.example",
        Phone = "+911234500001",
        IsActive = true
    };

    [Fact]
    public async Task LinkUserAsync_UnknownUser_ThrowsEntityNotFound()
    {
        var vendor = CreateVendor();
        var userId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.LinkUserAsync(vendor.Id, new VendorLinkUserRequest { UserId = userId }, CancellationToken.None));
        _vendorRepository.Verify(r => r.LinkUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LinkUserAsync_UserAlreadyLinkedToAnotherVendor_ThrowsBusiness()
    {
        var vendor = CreateVendor();
        var otherVendor = CreateVendor();
        var userId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = userId, FullName = "Vendor User", Email = "vendor@example.com" });
        _vendorRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(otherVendor);

        var sut = CreateSut();

        await Assert.ThrowsAsync<BusinessException>(() => sut.LinkUserAsync(vendor.Id, new VendorLinkUserRequest { UserId = userId }, CancellationToken.None));
        _vendorRepository.Verify(r => r.LinkUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LinkUserAsync_UnlinkedUser_Succeeds()
    {
        var vendor = CreateVendor();
        var userId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new UserModel { Id = userId, FullName = "Vendor User", Email = "vendor@example.com" });
        _vendorRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((VendorModel?)null);

        var sut = CreateSut();
        var result = await sut.LinkUserAsync(vendor.Id, new VendorLinkUserRequest { UserId = userId }, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        _vendorRepository.Verify(r => r.LinkUserAsync(vendor.Id, userId, It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReplaceRatesAsync_UnknownDestination_ThrowsEntityNotFound()
    {
        var vendor = CreateVendor();
        var destinationId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _destinationRepository.Setup(r => r.GetExistingIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut();
        var rates = new List<VendorRateRequest> { new() { DestinationId = destinationId, ServiceDescription = "Deluxe room", RateAmount = 5000 } };

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.ReplaceRatesAsync(vendor.Id, rates, CancellationToken.None));
        _vendorRepository.Verify(r => r.ReplaceRatesAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<VendorRateModel>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePaymentAsync_UnknownBooking_ThrowsEntityNotFound()
    {
        var vendor = CreateVendor();
        var bookingId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _bookingRepository.Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync((BookingModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.CreatePaymentAsync(vendor.Id, new VendorPaymentRequest { BookingId = bookingId, Amount = 1000 }, CancellationToken.None));
        _vendorRepository.Verify(r => r.CreatePaymentAsync(It.IsAny<VendorPaymentModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_PaymentBelongsToDifferentVendor_ThrowsEntityNotFound()
    {
        var vendor = CreateVendor();
        var payment = new VendorPaymentModel { Id = Guid.NewGuid(), VendorId = Guid.NewGuid(), Amount = 500, Status = "Pending" };
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _vendorRepository.Setup(r => r.GetPaymentByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.UpdatePaymentStatusAsync(vendor.Id, payment.Id, new VendorPaymentStatusRequest { Status = "Paid" }, CancellationToken.None));
        _vendorRepository.Verify(r => r.UpdatePaymentStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_MarkedPaid_SetsPaidAt()
    {
        var vendor = CreateVendor();
        var payment = new VendorPaymentModel { Id = Guid.NewGuid(), VendorId = vendor.Id, Amount = 500, Status = "Pending" };
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _vendorRepository.Setup(r => r.GetPaymentByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var sut = CreateSut();
        await sut.UpdatePaymentStatusAsync(vendor.Id, payment.Id, new VendorPaymentStatusRequest { Status = "Paid" }, CancellationToken.None);

        _vendorRepository.Verify(r => r.UpdatePaymentStatusAsync(payment.Id, "Paid", It.Is<DateTime?>(d => d.HasValue), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordPerformanceAsync_UnknownBooking_ThrowsEntityNotFound()
    {
        var vendor = CreateVendor();
        var bookingId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _bookingRepository.Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>())).ReturnsAsync((BookingModel?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.RecordPerformanceAsync(vendor.Id, new VendorPerformanceRequest { BookingId = bookingId, Rating = 4 }, CancellationToken.None));
        _vendorRepository.Verify(r => r.CreatePerformanceAsync(It.IsAny<VendorPerformanceModel>(), It.IsAny<CancellationToken>()), Times.Never);
        _vendorRepository.Verify(r => r.RecomputeRatingAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordPerformanceAsync_Valid_RecomputesRating()
    {
        var vendor = CreateVendor();
        _vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);
        _vendorRepository.Setup(r => r.GetPerformanceAsync(vendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<VendorPerformanceModel>)new List<VendorPerformanceModel>());

        var sut = CreateSut();
        await sut.RecordPerformanceAsync(vendor.Id, new VendorPerformanceRequest { Rating = 5, Notes = "Great support." }, CancellationToken.None);

        _vendorRepository.Verify(r => r.CreatePerformanceAsync(It.Is<VendorPerformanceModel>(p => p.VendorId == vendor.Id && p.Rating == 5), It.IsAny<CancellationToken>()), Times.Once);
        _vendorRepository.Verify(r => r.RecomputeRatingAsync(vendor.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
