namespace OneClickYatra.Api.Models;

/// <summary>An append-only gateway-event ledger row; never updated or deleted.</summary>
public sealed class PaymentTransactionModel
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? GatewayEventId { get; set; }
    public string? RawPayload { get; set; }
    public DateTime CreatedAt { get; set; }
}
