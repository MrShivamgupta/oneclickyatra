namespace OneClickYatra.Api.Models;

public sealed class PackageInventoryModel
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public DateOnly DepartureDate { get; set; }
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; }
    public string Status { get; set; } = "Open";

    public int AvailableSeats => TotalSeats - BookedSeats;
}
