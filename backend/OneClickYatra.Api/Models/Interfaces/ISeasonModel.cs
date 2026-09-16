namespace OneClickYatra.Api.Models.Interfaces;

public interface ISeasonModel
{
    Guid Id { get; }
    string Name { get; }
    int StartMonth { get; }
    int EndMonth { get; }
    bool IsDeleted { get; }
}
