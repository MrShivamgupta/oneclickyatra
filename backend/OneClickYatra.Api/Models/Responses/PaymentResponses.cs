namespace OneClickYatra.Api.Models.Responses;

public sealed class PaymentInitiateResponse
{
    public Guid PaymentId { get; set; }
    public string GatewayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string GatewayKeyId { get; set; } = string.Empty;
}

public sealed class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public string? CustomerName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = string.Empty;
    public string GatewayProvider { get; set; } = string.Empty;
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class RefundResponse
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? GatewayRefundId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class InvoiceResponse
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string? BookingNumber { get; set; }
    public string? CustomerName { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IssuedAt { get; set; }
}
