using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Security.Jwt;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> __options)
    {
        _options = __options.Value;
    }

    public AccessTokenResult GenerateAccessToken(JwtClaimsInput __input)
    {
        if (string.IsNullOrWhiteSpace(_options.Key))
        {
            throw new ConfigurationException("Authentication is not fully configured on this environment. Please try again shortly.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, __input.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, __input.Email),
            new("full_name", __input.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Emit both: ClaimTypes.Role so ASP.NET Core's User.IsInRole()/[Authorize(Roles=...)] keep
        // working server-side (that's the default RoleClaimType), and a short "role" claim so
        // frontend JWT decoding doesn't need to know that long URI.
        claims.AddRange(__input.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(__input.Roles.Select(role => new Claim("role", role)));
        claims.AddRange(__input.Permissions.Select(permission => new Claim("permission", permission)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
