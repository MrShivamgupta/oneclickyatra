namespace OneClickYatra.Api.Models.Interfaces;

public interface ILeadModel
{
    Guid Id { get; }
    string CustomerName { get; }
    string Mobile { get; }
    string? Email { get; }
    Guid? DestinationId { get; }
    DateOnly? TravelDate { get; }
    decimal? Budget { get; }
    string? Source { get; }
    Guid? AssignedToUserId { get; }
    int LeadScore { get; }
    string Status { get; }
    Guid? CustomerId { get; }
    bool IsDeleted { get; }
}
