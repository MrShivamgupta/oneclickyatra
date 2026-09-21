using Microsoft.Extensions.Logging;
using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.UnitTests.AppFunctions;

public class VendorPortalAppFunctionTests
{
    private readonly Mock<IVendorRepository> _vendorRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IVendorInvoiceRepository> _vendorInvoiceRepository = new();
    private readonly Mock<IFileStorageService> _fileStorageService = new();
    private readonly Mock<IAuditLogWriter> _auditLogWriter = new();

    private VendorPortalAppFunction CreateSut() => new(
        _vendorRepository.Object,
        _userRepository.Object,
        _vendorInvoiceRepository.Object,
        _fileStorageService.Object,
        _auditLogWriter.Object,
        Mock.Of<ILogger<VendorPortalAppFunction>>());

    [Fact]
    public async Task GetMyProfileAsync_NoLinkedVendorYet_CreatesPlaceholderFromUser()
    {
        var userId = Guid.NewGuid();
        _vendorRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((VendorModel?)null);
        _userRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserModel { Id = userId, Email = "vendor@example.com", FullName = "New Vendor" });

        var sut = CreateSut();
        var result = await sut.GetMyProfileAsync(userId, CancellationToken.None);

        Assert.Equal("vendor@example.com", result.Email);
        Assert.False(result.IsActive);
        _vendorRepository.Verify(r => r.CreateAsync(
            It.Is<VendorModel>(v => v.UserId == userId && v.VendorType == "Hotel" && !v.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyProfileAsync_AlreadyLinked_ReturnsExistingVendorWithoutCreating()
    {
        var userId = Guid.NewGuid();
        var vendor = new VendorModel { Id = Guid.NewGuid(), Name = "Blue Lagoon Resorts", VendorType = "Hotel", Email = "ops@bluelagoon.example", Phone = "+911234500001", UserId = userId, IsActive = true };
        _vendorRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);

        var sut = CreateSut();
        var result = await sut.GetMyProfileAsync(userId, CancellationToken.None);

        Assert.Equal(vendor.Id, result.Id);
        _vendorRepository.Verify(r => r.CreateAsync(It.IsAny<VendorModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_DoesNotChangeVendorTypeOrActivation()
    {
        var userId = Guid.NewGuid();
        var vendor = new VendorModel { Id = Guid.NewGuid(), Name = "Blue Lagoon Resorts", VendorType = "Hotel", Email = "ops@bluelagoon.example", Phone = "+911234500001", UserId = userId, IsActive = true };
        _vendorRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);

        var sut = CreateSut();
        var result = await sut.UpdateMyProfileAsync(userId, new VendorRequest
        {
            Name = "Blue Lagoon Resorts & Spa",
            VendorType = "Airline",
            Email = "ops@bluelagoon.example",
            Phone = "+911234500002",
            IsActive = false
        }, CancellationToken.None);

        Assert.Equal("Blue Lagoon Resorts & Spa", result.Name);
        Assert.Equal("+911234500002", result.Phone);
        Assert.Equal("Hotel", result.VendorType);
        Assert.True(result.IsActive);
        _vendorRepository.Verify(r => r.UpdateAsync(It.Is<VendorModel>(v => v.VendorType == "Hotel" && v.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }
}
