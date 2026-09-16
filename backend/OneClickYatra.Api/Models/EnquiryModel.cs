using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class EnquiryModel : IEnquiryModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public DateOnly? TravelDate { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "New";
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Populated by a JOIN in read queries; not a physical column.
    public string? DestinationName { get; set; }
}
