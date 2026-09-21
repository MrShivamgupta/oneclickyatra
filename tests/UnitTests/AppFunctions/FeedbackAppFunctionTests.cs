using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.AppFunctions;

public class FeedbackAppFunctionTests
{
    private readonly Mock<IFeedbackRepository> _feedbackRepository = new();

    private FeedbackAppFunction CreateSut() => new(_feedbackRepository.Object);

    [Fact]
    public async Task SearchAsync_RatingRangeProvided_PassesThemThroughUnchanged()
    {
        FeedbackSearchRequest? captured = null;
        _feedbackRepository
            .Setup(r => r.SearchAsync(It.IsAny<FeedbackSearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<FeedbackSearchRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(PaginationResponse<FeedbackModel>.Create(new List<FeedbackModel>(), 1, 20, 0));

        var sut = CreateSut();
        var request = new FeedbackSearchRequest { MinRating = 3, MaxRating = 5 };
        await sut.SearchAsync(request, CancellationToken.None);

        Assert.Equal(3, captured?.MinRating);
        Assert.Equal(5, captured?.MaxRating);
        _feedbackRepository.Verify(r => r.SearchAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_NoFiltersProvided_DefaultsToFirstPageOfTwenty()
    {
        FeedbackSearchRequest? captured = null;
        _feedbackRepository
            .Setup(r => r.SearchAsync(It.IsAny<FeedbackSearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<FeedbackSearchRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(PaginationResponse<FeedbackModel>.Create(new List<FeedbackModel>(), 1, 20, 0));

        var sut = CreateSut();
        await sut.SearchAsync(new FeedbackSearchRequest(), CancellationToken.None);

        Assert.Equal(1, captured?.PageNumber);
        Assert.Equal(20, captured?.PageSize);
        Assert.Null(captured?.MinRating);
        Assert.Null(captured?.MaxRating);
        Assert.Null(captured?.FromDate);
        Assert.Null(captured?.ToDate);
    }

    [Fact]
    public async Task SearchAsync_MapsRepositoryRowsToResponse()
    {
        var feedback = new FeedbackModel
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            BookingNumber = "BK-1001",
            CustomerName = "Jane Doe",
            Rating = 4,
            Comment = "Great trip overall.",
            CreatedAt = new DateTime(2026, 2, 10)
        };
        _feedbackRepository
            .Setup(r => r.SearchAsync(It.IsAny<FeedbackSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaginationResponse<FeedbackModel>.Create(new List<FeedbackModel> { feedback }, 1, 20, 1));

        var sut = CreateSut();
        var result = await sut.SearchAsync(new FeedbackSearchRequest(), CancellationToken.None);

        var row = Assert.Single(result.Items);
        Assert.Equal(feedback.Id, row.Id);
        Assert.Equal("BK-1001", row.BookingNumber);
        Assert.Equal("Jane Doe", row.CustomerName);
        Assert.Equal(4, row.Rating);
        Assert.Equal("Great trip overall.", row.Comment);
        Assert.Equal(1, result.TotalCount);
    }
}
