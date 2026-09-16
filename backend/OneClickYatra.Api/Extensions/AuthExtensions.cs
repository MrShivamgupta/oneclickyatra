using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OneClickYatra.Api.Security.Authorization;
using OneClickYatra.Api.Security.Jwt;

namespace OneClickYatra.Api.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection __services, IConfiguration __configuration)
    {
        var jwtSection = __configuration.GetSection(JwtOptions.SectionName);
        __services.Configure<JwtOptions>(jwtSection);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        var signingKey = string.IsNullOrWhiteSpace(jwtOptions.Key)
            ? new byte[32] // placeholder key so the app can still start in environments where Jwt:Key has not been set yet (e.g. first-run before user-secrets are configured); tokens issued/validated with it will not match a properly configured key.
            : Encoding.UTF8.GetBytes(jwtOptions.Key);

        __services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        __services.AddAuthorizationBuilder();
        __services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionAuthorizationHandler>();

        return __services;
    }
}
