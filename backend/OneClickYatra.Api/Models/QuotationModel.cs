namespace OneClickYatra.Api.Models;

/// <summary>Maps 1:1 to the Quotations table, plus a few joined display-only fields populated by the repository.</summary>
public sealed class QuotationModel
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public Guid LeadId { get; set; }
    public Guid? CustomerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateOnly? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string PublicTokenHash { get; set; } = string.Empty;
    public Guid? SelectedOptionId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public string? LeadCustomerName { get; set; }
    public string? CustomerName { get; set; }
}
