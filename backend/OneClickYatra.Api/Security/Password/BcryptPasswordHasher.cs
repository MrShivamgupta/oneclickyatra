namespace OneClickYatra.Api.Security.Password;

/// <summary>BCrypt-based password hashing. Passwords are never stored or logged in plain text.</summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string __plainTextPassword) =>
        BCrypt.Net.BCrypt.HashPassword(__plainTextPassword, workFactor: WorkFactor);

    public bool Verify(string __plainTextPassword, string __passwordHash) =>
        BCrypt.Net.BCrypt.Verify(__plainTextPassword, __passwordHash);
}
