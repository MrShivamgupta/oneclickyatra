namespace OneClickYatra.Api.Security;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public int PasswordResetTokenMinutes { get; set; } = 30;
}
