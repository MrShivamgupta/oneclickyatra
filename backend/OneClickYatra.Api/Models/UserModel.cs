using OneClickYatra.Api.Models.Interfaces;

namespace OneClickYatra.Api.Models;

/// <summary>Maps 1:1 to the Users table. Populated by Dapper (property names match column names).</summary>
public sealed class UserModel : IUserModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedOutUntilUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Populated only by UserRepository.SearchAsync's join/STRING_AGG to UserRoles+Roles
    /// (comma-separated staff role names for this user). Ignored by CreateAsync's INSERT and by
    /// GetByEmailAsync/GetByIdAsync/ListStaffAsync, which don't select this column.</summary>
    public string? RoleNames { get; set; }
}
