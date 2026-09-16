namespace OneClickYatra.Api.Models.Responses;

public sealed class QuotationItemResponse
{
    public Guid? Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}

public sealed class QuotationOptionResponse
{
    public Guid? Id { get; set; }
    public Guid? PackageId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    public int? DurationDays { get; set; }
    public int? DurationNights { get; set; }
    public string? HotelCategory { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal PricePerPerson { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsRecommended { get; set; }
    public int SortOrder { get; set; }
    public List<QuotationItemResponse> Items { get; set; } = [];
}

public sealed class QuotationResponse
{
    public Guid Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public Guid LeadId { get; set; }
    public string? LeadCustomerName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public string? RejectionReason { get; set; }

    /// <summary>The raw (unhashed) share token. Only ever populated by Create and RegenerateLink —
    /// the DB stores just a hash, so this cannot be recovered any other time.</summary>
    public string? PublicToken { get; set; }
}

public sealed class QuotationDetailResponse
{
    public QuotationResponse Quotation { get; set; } = new();
    public List<QuotationOptionResponse> Options { get; set; } = [];
}

public sealed class QuotationPublicResponse
{
    public string QuotationNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly? ValidUntil { get; set; }
    public string? Notes { get; set; }
    public string? LeadCustomerName { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public List<QuotationOptionResponse> Options { get; set; } = [];
}
