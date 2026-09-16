using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class LeadModel : ILeadModel
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? DestinationId { get; set; }
    public DateOnly? TravelDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public int LeadScore { get; set; }
    public string Status { get; set; } = "New";
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Populated by JOINs in read queries; not physical columns.
    public string? DestinationName { get; set; }
    public string? AssignedToName { get; set; }
}
