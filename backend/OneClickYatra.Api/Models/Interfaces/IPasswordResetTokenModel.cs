namespace OneClickYatra.Api.Models.Interfaces;

public interface IPasswordResetTokenModel
{
    Guid Id { get; }
    Guid UserId { get; }
    string TokenHash { get; }
    DateTime ExpiresAtUtc { get; }
    DateTime? UsedAtUtc { get; }
}
