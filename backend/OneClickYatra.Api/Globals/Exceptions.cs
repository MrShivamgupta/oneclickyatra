namespace OneClickYatra.Api.Globals;

/// <summary>Thrown by AppFunctions/AppValidations for request validation failures (maps to 422).</summary>
public sealed class ValidationAppException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IReadOnlyDictionary<string, string[]> __errors)
        : base("Validation failed.")
    {
        Errors = __errors;
    }

    public ValidationAppException(string __field, string __error)
        : this(new Dictionary<string, string[]> { [__field] = new[] { __error } })
    {
    }
}

/// <summary>Thrown for expected business-rule violations (maps to 409/400 depending on context).</summary>
public sealed class BusinessException : Exception
{
    public BusinessException(string __message) : base(__message)
    {
    }
}

/// <summary>Thrown when a requested entity does not exist (maps to 404).</summary>
public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string __entityName, object __key)
        : base($"{__entityName} with key '{__key}' was not found.")
    {
    }
}

/// <summary>Thrown for a failed login/refresh attempt (maps to 401). Message is intentionally generic.</summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email or password.")
    {
    }
}

/// <summary>Thrown when an account is temporarily locked out after too many failed logins (maps to 423).</summary>
public sealed class AccountLockedException : Exception
{
    public AccountLockedException(DateTime __lockedUntilUtc)
        : base($"Account is temporarily locked until {__lockedUntilUtc:O} due to repeated failed login attempts.")
    {
    }
}

/// <summary>Thrown when a webhook's signature cannot be verified against the configured secret (maps to 400).</summary>
public sealed class InvalidWebhookSignatureException : Exception
{
    public InvalidWebhookSignatureException() : base("Webhook signature verification failed.")
    {
    }
}

/// <summary>Thrown when a required environment setting (a connection string, the JWT signing key,
/// etc.) is missing or malformed — a deployment/configuration problem, not a client error or a bug
/// in a single request. Maps to 503 (same tier as PaymentGatewayNotConfiguredException/
/// WhatsAppNotConfiguredException) so it's visibly distinct from the generic 500 fallback and never
/// leaks the missing setting's name or value to the client.</summary>
public sealed class ConfigurationException : Exception
{
    public ConfigurationException(string __message) : base(__message)
    {
    }
}
