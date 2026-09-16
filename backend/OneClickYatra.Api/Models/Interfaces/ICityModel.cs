namespace OneClickYatra.Api.Models.Interfaces;

public interface ICityModel
{
    Guid Id { get; }
    Guid CountryId { get; }
    string Name { get; }
    bool IsDeleted { get; }
}
