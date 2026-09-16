namespace OneClickYatra.Api.Services.Payments;

/// <summary>Thrown when a call to the gateway's API fails (non-success HTTP status, malformed response).</summary>
public sealed class PaymentGatewayException : Exception
{
    public PaymentGatewayException(string __message) : base(__message)
    {
    }
}
