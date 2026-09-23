namespace OneClickYatra.Api.Models.Responses;

/// <summary>Read-only view of whether each pluggable external integration has real credentials
/// configured in this environment (appsettings/env vars) -- never the credential values themselves.
/// These are deliberately not editable here: secrets belong in appsettings/env, not a DB-backed
/// settings UI (see docs/PROJECT_PLAN.md's Environment Notes for why this page doesn't try to be a
/// secrets manager).</summary>
public sealed class IntegrationStatusResponse
{
    public bool RazorpayConfigured { get; set; }
    public bool WhatsAppConfigured { get; set; }
    public bool EmailConfigured { get; set; }
}
