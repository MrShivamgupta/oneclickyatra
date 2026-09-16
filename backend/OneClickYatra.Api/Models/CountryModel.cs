using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class CountryModel : ICountryModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
