namespace OneClickYatra.Api.Models.Interfaces;

public interface IUserModel
{
    Guid Id { get; }
    string Email { get; }
    string FullName { get; }
    string PasswordHash { get; }
    bool IsActive { get; }
    int FailedLoginAttempts { get; }
    DateTime? LockedOutUntilUtc { get; }
    DateTime CreatedAt { get; }
    Guid? CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
    bool IsDeleted { get; }
}
