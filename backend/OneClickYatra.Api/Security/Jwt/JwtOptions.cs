namespace OneClickYatra.Api.Security.Jwt;

/// <summary>
/// Bound from the "Jwt" configuration section. The signing Key must never be committed —
/// set it via environment variable (Jwt__Key) or `dotnet user-secrets set "Jwt:Key" "..."`.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
