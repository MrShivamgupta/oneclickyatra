namespace OneClickYatra.Api.Security.Password;

public interface IPasswordHasher
{
    string Hash(string __plainTextPassword);
    bool Verify(string __plainTextPassword, string __passwordHash);
}
