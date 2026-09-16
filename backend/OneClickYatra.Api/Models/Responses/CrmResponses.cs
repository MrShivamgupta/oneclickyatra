namespace OneClickYatra.Api.Models.Responses;

public sealed class CustomerResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
}

public sealed class LeadResponse
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public DateOnly? TravelDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Source { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public int LeadScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
}

public sealed class FollowUpResponse
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string? LeadCustomerName { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}
