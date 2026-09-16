namespace OneClickYatra.Api.Security.Jwt;

public sealed record JwtClaimsInput(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(JwtClaimsInput __input);
    string GenerateRefreshToken();
}
