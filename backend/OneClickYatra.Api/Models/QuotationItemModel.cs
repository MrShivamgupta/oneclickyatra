namespace OneClickYatra.Api.Models;

public sealed class QuotationItemModel
{
    public Guid Id { get; set; }
    public Guid QuotationOptionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}
