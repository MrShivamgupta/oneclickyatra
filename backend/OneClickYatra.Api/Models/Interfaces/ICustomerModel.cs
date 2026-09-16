namespace OneClickYatra.Api.Models.Interfaces;

public interface ICustomerModel
{
    Guid Id { get; }
    string FullName { get; }
    string? Email { get; }
    string Phone { get; }
    Guid? UserId { get; }
    bool IsDeleted { get; }
}
