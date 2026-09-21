namespace OneClickYatra.Api.Models.Responses;

public sealed class VendorInvoiceResponse
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
