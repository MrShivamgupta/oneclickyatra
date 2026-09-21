using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

/// <summary>Filters for the admin "Users" (staff accounts) search. Only ever matches non-Customer/
/// non-Guest/non-Vendor users — Customers and Vendors have their own admin screens already.
/// SearchTerm (inherited from PaginationRequest) matches FullName or Email.</summary>
public sealed class UserSearchRequest : PaginationRequest
{
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>Creates a new staff account. Role must be one of TravelAgent/OperationsStaff/Finance/
/// SuperAdmin (enforced by CreateStaffUserRequestValidator) — Customer/Guest/Vendor accounts are
/// created through their own existing flows and are out of scope here. Password is hashed the same
/// way as self-registration (see AuthAppFunction.RegisterAsync); the new account always starts active.</summary>
public sealed class CreateStaffUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>Changes a staff user's role: their current staff role(s) are removed and this one is
/// assigned. Same TravelAgent/OperationsStaff/Finance/SuperAdmin-only validation as
/// CreateStaffUserRequest (enforced by UpdateUserRoleRequestValidator).</summary>
public sealed class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

/// <summary>Activates or deactivates a staff user. There is no hard delete for users.</summary>
public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}
