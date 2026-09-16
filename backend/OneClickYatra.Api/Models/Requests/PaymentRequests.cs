using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class PaymentInitiateRequest
{
    public Guid BookingId { get; set; }
}

public sealed class PaymentRefundRequest
{
    public Guid BookingId { get; set; }
    public string? Reason { get; set; }
}

public sealed class PaymentSearchRequest : PaginationRequest
{
    public string? Status { get; set; }
    public Guid? BookingId { get; set; }
}

public sealed class InvoiceSearchRequest : PaginationRequest
{
    public Guid? BookingId { get; set; }
}
