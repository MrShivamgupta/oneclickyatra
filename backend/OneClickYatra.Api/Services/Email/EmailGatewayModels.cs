namespace OneClickYatra.Api.Services.Email;

/// <summary>Thrown when a call to the SMTP server fails (connection, authentication or send
/// failure). The message never includes the SmtpPassword.</summary>
public sealed class EmailGatewayException : Exception
{
    public EmailGatewayException(string __message) : base(__message)
    {
    }
}

/// <summary>Thrown when an email send is attempted without the SMTP gateway being configured (no
/// SmtpHost set) -- mapped by the exception middleware like any other well-known exception, never
/// leaking configuration details to the client.</summary>
public sealed class EmailNotConfiguredException : Exception
{
    public EmailNotConfiguredException()
        : base("The email (SMTP) gateway is not configured on this environment.")
    {
    }
}
