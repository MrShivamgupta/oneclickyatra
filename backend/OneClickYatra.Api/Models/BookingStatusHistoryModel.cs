namespace OneClickYatra.Api.Models;

/// <summary>An append-only audit row; one is written per status transition and never mutated.</summary>
public sealed class BookingStatusHistoryModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
    public string? TrackingId { get; set; }

    public string? ChangedByName { get; set; }
}
