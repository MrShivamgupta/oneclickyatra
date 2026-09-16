namespace OneClickYatra.Api.Models;

/// <summary>An immutable audit record of a customer's decision on a quotation, made via the public link.</summary>
public sealed class QuotationApprovalModel
{
    public Guid Id { get; set; }
    public Guid QuotationId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? ApprovedByName { get; set; }
    public string? Comments { get; set; }
    public string? IpAddress { get; set; }
    public DateTime DecidedAt { get; set; }
}
