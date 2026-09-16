namespace OneClickYatra.Api.Models.Responses;

/// <summary>Minimal staff projection used by pickers (e.g. "Assign lead to"). Never includes PasswordHash or other sensitive fields.</summary>
public sealed class UserSummaryResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
