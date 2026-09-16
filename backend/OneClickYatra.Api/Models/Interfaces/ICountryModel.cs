namespace OneClickYatra.Api.Models.Interfaces;

public interface ICountryModel
{
    Guid Id { get; }
    string Name { get; }
    string IsoCode { get; }
    bool IsDeleted { get; }
}
