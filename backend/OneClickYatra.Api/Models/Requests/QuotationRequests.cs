using OneClickYatra.Api.Globals;

namespace OneClickYatra.Api.Models.Requests;

public sealed class QuotationRequest
{
    public Guid LeadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? ValidUntil { get; set; }
    public string? Notes { get; set; }
}

public sealed class QuotationSearchRequest : PaginationRequest
{
    public string? Status { get; set; }
    public Guid? LeadId { get; set; }
}

public sealed class QuotationOptionItemRequest
{
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QuotationOptionRequest
{
    public Guid? PackageId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public int? DurationDays { get; set; }
    public int? DurationNights { get; set; }
    public string? HotelCategory { get; set; }
    public int NumberOfPeople { get; set; } = 1;
    public decimal PricePerPerson { get; set; }
    public bool IsRecommended { get; set; }
    public int SortOrder { get; set; }
    public List<QuotationOptionItemRequest> Items { get; set; } = [];
}

public sealed class QuotationPublicApproveRequest
{
    public Guid SelectedOptionId { get; set; }
    public string ApprovedByName { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

public sealed class QuotationPublicRejectRequest
{
    public string RejectedByName { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
