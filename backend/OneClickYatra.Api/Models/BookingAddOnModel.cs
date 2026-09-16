namespace OneClickYatra.Api.Models;

public sealed class BookingAddOnModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}
