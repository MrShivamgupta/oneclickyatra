namespace OneClickYatra.Api.Models;

public sealed class AgencyProfileModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public string? GstNumber { get; set; }
    public string Currency { get; set; } = "INR";
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
