namespace OneClickYatra.Api.Models;

public sealed class BookingPassengerModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? IdProofType { get; set; }
    public string? IdProofNumber { get; set; }
    public bool IsLeadPassenger { get; set; }
}
