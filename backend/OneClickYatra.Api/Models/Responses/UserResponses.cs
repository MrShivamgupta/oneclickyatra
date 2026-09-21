namespace OneClickYatra.Api.Models.Responses;

/// <summary>Minimal staff projection used by pickers (e.g. "Assign lead to"). Never includes PasswordHash or other sensitive fields.</summary>
public sealed class UserSummaryResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>Full admin projection of a staff user for the "Users" management screen. Never includes
/// PasswordHash or other sensitive fields.</summary>
public sealed class UserResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
