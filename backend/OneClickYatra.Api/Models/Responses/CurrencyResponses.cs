namespace OneClickYatra.Api.Models.Responses;

public sealed class CurrencyRatesResponse
{
    public string Base { get; set; } = "INR";
    public DateTime AsOf { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = new();
}
