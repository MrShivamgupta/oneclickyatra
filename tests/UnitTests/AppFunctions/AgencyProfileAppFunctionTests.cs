using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.AppFunctions;

public class AgencyProfileAppFunctionTests
{
    private readonly Mock<IAgencyProfileRepository> _agencyProfileRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();

    private AgencyProfileAppFunction CreateSut() => new(_agencyProfileRepository.Object, _currentUserAccessor.Object);

    private static AgencyProfileModel MakeProfile() => new()
    {
        Id = Guid.NewGuid(),
        Name = "One Click Yatra",
        Currency = "INR",
        SupportEmail = "support@oneclickyatra.dev"
    };

    [Fact]
    public async Task GetAsync_RowMissing_ThrowsEntityNotFound()
    {
        _agencyProfileRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((AgencyProfileModel?)null);

        var sut = CreateSut();
        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.GetAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_RowExists_MapsToResponse()
    {
        var profile = MakeProfile();
        _agencyProfileRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var sut = CreateSut();
        var result = await sut.GetAsync(CancellationToken.None);

        Assert.Equal(profile.Name, result.Name);
        Assert.Equal(profile.Currency, result.Currency);
        Assert.Equal(profile.SupportEmail, result.SupportEmail);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFieldsAndStampsUpdatedBy()
    {
        var caller = Guid.NewGuid();
        var profile = MakeProfile();
        _agencyProfileRepository.SetupSequence(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile)
            .ReturnsAsync(profile);
        _currentUserAccessor.Setup(a => a.UserId).Returns(caller);

        var sut = CreateSut();
        var request = new UpdateAgencyProfileRequest
        {
            Name = "Updated Agency",
            LogoUrl = "https://example.com/logo.png",
            Address = "123 Main St",
            GstNumber = "GST123",
            Currency = "USD",
            SupportEmail = "help@example.com",
            SupportPhone = "+1234567890"
        };
        await sut.UpdateAsync(request, CancellationToken.None);

        _agencyProfileRepository.Verify(r => r.UpdateAsync(It.Is<AgencyProfileModel>(p =>
            p.Name == "Updated Agency" &&
            p.LogoUrl == "https://example.com/logo.png" &&
            p.Address == "123 Main St" &&
            p.GstNumber == "GST123" &&
            p.Currency == "USD" &&
            p.SupportEmail == "help@example.com" &&
            p.SupportPhone == "+1234567890" &&
            p.UpdatedBy == caller
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RowMissing_ThrowsEntityNotFound()
    {
        _agencyProfileRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((AgencyProfileModel?)null);

        var sut = CreateSut();
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.UpdateAsync(new UpdateAgencyProfileRequest { Name = "X", Currency = "INR" }, CancellationToken.None));
        _agencyProfileRepository.Verify(r => r.UpdateAsync(It.IsAny<AgencyProfileModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
