namespace OneClickYatra.Api.Models;

public sealed class RefundModel
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Requested";
    public string? GatewayRefundId { get; set; }
    public Guid? RequestedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
