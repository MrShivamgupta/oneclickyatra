namespace OneClickYatra.Api.Models.Interfaces;

public interface IPackageModel
{
    Guid Id { get; }
    Guid DestinationId { get; }
    Guid? CategoryId { get; }
    Guid? SeasonId { get; }
    string Title { get; }
    string Slug { get; }
    int DurationDays { get; }
    int DurationNights { get; }
    string? ShortDescription { get; }
    string? Description { get; }
    string? HeroImageUrl { get; }
    string Status { get; }
    bool IsDeleted { get; }
}
