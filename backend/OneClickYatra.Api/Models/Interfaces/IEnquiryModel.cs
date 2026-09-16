namespace OneClickYatra.Api.Models.Interfaces;

public interface IEnquiryModel
{
    Guid Id { get; }
    string FullName { get; }
    string Email { get; }
    string Phone { get; }
    Guid? DestinationId { get; }
    DateOnly? TravelDate { get; }
    string? Message { get; }
    string Status { get; }
    bool IsDeleted { get; }
}
