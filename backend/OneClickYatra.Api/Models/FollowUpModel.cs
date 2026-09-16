using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class FollowUpModel : IFollowUpModel
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = "Call";
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Populated by JOINs in read queries; not physical columns.
    public string? LeadCustomerName { get; set; }
}
