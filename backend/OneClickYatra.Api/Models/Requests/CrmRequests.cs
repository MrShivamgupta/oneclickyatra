using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class CustomerRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
}

public sealed class LeadRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? DestinationId { get; set; }
    public DateOnly? TravelDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedToUserId { get; set; }
}

public sealed class LeadSearchRequest : PaginationRequest
{
    public string? Status { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? DestinationId { get; set; }
}

public sealed class LeadStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class LeadAssignRequest
{
    public Guid AssignedToUserId { get; set; }
}

public sealed class LeadScoreRequest
{
    public int LeadScore { get; set; }
}

public sealed class FollowUpRequest
{
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = "Call";
    public string? Notes { get; set; }
}

public sealed class FollowUpStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
