using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using OneClickYatra.Api.Security.Jwt;

namespace OneClickYatra.UnitTests.Security;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int accessTokenMinutes = 15) =>
        new(Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "unit-test-signing-key-unit-test-signing-key", // 32+ chars, test-only
            AccessTokenMinutes = accessTokenMinutes,
            RefreshTokenDays = 7
        }));

    [Fact]
    public void GenerateAccessToken_IncludesRolesAndPermissionsAsClaims()
    {
        var service = CreateService();
        var input = new JwtClaimsInput(
            Guid.NewGuid(), "user@example.com", "Test User",
            Roles: ["TravelAgent"], Permissions: ["lead.view", "lead.create"]);

        var result = service.GenerateAccessToken(input);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        Assert.Contains(token.Claims, c => c.Type == "permission" && c.Value == "lead.view");
        Assert.Contains(token.Claims, c => c.Type == "permission" && c.Value == "lead.create");
        // Short "role" claim must exist so frontend JWT decoding (which reads this exact key,
        // not the long ClaimTypes.Role URI) can resolve the user's roles.
        Assert.Contains(token.Claims, c => c.Type == "role" && c.Value == "TravelAgent");
    }

    [Fact]
    public void GenerateAccessToken_SetsExpiryAccordingToConfiguration()
    {
        var service = CreateService(accessTokenMinutes: 30);
        var input = new JwtClaimsInput(Guid.NewGuid(), "user@example.com", "Test User", [], []);

        var before = DateTime.UtcNow;
        var result = service.GenerateAccessToken(input);

        Assert.True(result.ExpiresAtUtc > before.AddMinutes(29));
        Assert.True(result.ExpiresAtUtc < before.AddMinutes(31));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentValuesEachCall()
    {
        var service = CreateService();

        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();

        Assert.NotEqual(first, second);
    }
}
