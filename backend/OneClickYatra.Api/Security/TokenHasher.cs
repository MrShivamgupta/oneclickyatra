using System.Security.Cryptography;
using System.Text;

namespace OneClickYatra.Api.Security;

/// <summary>Opaque tokens (refresh tokens, password reset tokens) are stored as SHA-256 hashes, never in plain text.</summary>
public static class TokenHasher
{
    public static string Hash(string __rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(__rawToken));
        return Convert.ToHexString(bytes);
    }
}
