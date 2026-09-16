namespace OneClickYatra.Api.Models;

public sealed class InvoiceModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public string? BookingNumber { get; set; }
    public string? CustomerName { get; set; }
}
