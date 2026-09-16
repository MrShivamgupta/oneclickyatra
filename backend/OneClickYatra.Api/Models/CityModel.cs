using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

public sealed class CityModel : ICityModel
{
    public Guid Id { get; set; }
    public Guid CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Populated by a JOIN in read queries; not a physical column.
    public string? CountryName { get; set; }
}
