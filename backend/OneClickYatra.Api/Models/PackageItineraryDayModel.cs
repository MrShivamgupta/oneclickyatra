namespace OneClickYatra.Api.Models;

public sealed class PackageItineraryDayModel
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public int DayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
