using Moq;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Repositories;

namespace OneClickYatra.UnitTests.AppFunctions;

public class AuditLogAppFunctionTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepository = new();

    private AuditLogAppFunction CreateSut() => new(_auditLogRepository.Object);

    [Fact]
    public async Task SearchAsync_FiltersProvided_PassesThemThroughUnchangedToRepository()
    {
        var request = new AuditLogSearchRequest
        {
            UserId = Guid.NewGuid(),
            Action = "booking",
            EntityName = "Booking",
            FromDate = new DateOnly(2026, 1, 1),
            ToDate = new DateOnly(2026, 1, 31)
        };
        _auditLogRepository
            .Setup(r => r.SearchAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaginationResponse<AuditLogModel>.Create(new List<AuditLogModel>(), request.PageNumber, request.PageSize, 0));

        var sut = CreateSut();
        await sut.SearchAsync(request, CancellationToken.None);

        _auditLogRepository.Verify(r => r.SearchAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_NoFiltersProvided_DefaultsToFirstPageOfTwenty()
    {
        AuditLogSearchRequest? captured = null;
        _auditLogRepository
            .Setup(r => r.SearchAsync(It.IsAny<AuditLogSearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLogSearchRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(PaginationResponse<AuditLogModel>.Create(new List<AuditLogModel>(), 1, 20, 0));

        var sut = CreateSut();
        await sut.SearchAsync(new AuditLogSearchRequest(), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(1, captured!.PageNumber);
        Assert.Equal(20, captured.PageSize);
        Assert.Null(captured.UserId);
        Assert.Null(captured.Action);
        Assert.Null(captured.EntityName);
        Assert.Null(captured.FromDate);
        Assert.Null(captured.ToDate);
    }

    [Fact]
    public async Task SearchAsync_ActingUserPresent_CombinesActorNameAndEmailInResponse()
    {
        var userId = Guid.NewGuid();
        _auditLogRepository
            .Setup(r => r.SearchAsync(It.IsAny<AuditLogSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaginationResponse<AuditLogModel>.Create(new List<AuditLogModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = "booking.created",
                    EntityName = "Booking",
                    EntityId = "BK-1",
                    ActorName = "Jane Doe",
                    ActorEmail = "jane@example.com",
                    CreatedAt = new DateTime(2026, 1, 5)
                }
            }, 1, 20, 1));

        var sut = CreateSut();
        var result = await sut.SearchAsync(new AuditLogSearchRequest(), CancellationToken.None);

        var row = Assert.Single(result.Items);
        Assert.Equal(userId, row.ActorUserId);
        Assert.Equal("Jane Doe (jane@example.com)", row.ActorName);
        Assert.Equal("booking.created", row.Action);
    }

    [Fact]
    public async Task SearchAsync_NoActingUser_ActorFieldsAreNullInResponse()
    {
        _auditLogRepository
            .Setup(r => r.SearchAsync(It.IsAny<AuditLogSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaginationResponse<AuditLogModel>.Create(new List<AuditLogModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = null,
                    Action = "tokens.cleanup",
                    EntityName = "RefreshToken",
                    ActorName = null,
                    ActorEmail = null,
                    CreatedAt = DateTime.UtcNow
                }
            }, 1, 20, 1));

        var sut = CreateSut();
        var result = await sut.SearchAsync(new AuditLogSearchRequest(), CancellationToken.None);

        var row = Assert.Single(result.Items);
        Assert.Null(row.ActorUserId);
        Assert.Null(row.ActorName);
    }
}
