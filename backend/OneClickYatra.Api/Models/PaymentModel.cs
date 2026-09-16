namespace OneClickYatra.Api.Models;

public sealed class PaymentModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = "Created";
    public string GatewayProvider { get; set; } = "Razorpay";
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    public string? BookingNumber { get; set; }
    public string? CustomerName { get; set; }
}
