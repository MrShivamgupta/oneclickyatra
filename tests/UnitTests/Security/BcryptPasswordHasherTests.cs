using OneClickYatra.Api.Security.Password;

namespace OneClickYatra.UnitTests.Security;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Correct-Password-1");

        Assert.True(_hasher.Verify("Correct-Password-1", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("Correct-Password-1");

        Assert.False(_hasher.Verify("Wrong-Password", hash));
    }

    [Fact]
    public void Hash_NeverReturnsThePlainTextPassword()
    {
        var hash = _hasher.Hash("Correct-Password-1");

        Assert.DoesNotContain("Correct-Password-1", hash);
    }
}
