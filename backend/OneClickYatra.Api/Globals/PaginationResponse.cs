namespace OneClickYatra.Api.Globals;

/// <summary>Standard paged-list shape returned inside ApiResponse&lt;T&gt;.Data for list endpoints.</summary>
public sealed class PaginationResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public long TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PaginationResponse<T> Create(IReadOnlyList<T> __items, int __pageNumber, int __pageSize, long __totalCount) => new()
    {
        Items = __items,
        PageNumber = __pageNumber,
        PageSize = __pageSize,
        TotalCount = __totalCount
    };
}

/// <summary>Common request shape for server-side pagination, sorting and filtering.</summary>
public class PaginationRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 20 : Math.Min(value, MaxPageSize);
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }

    public int Skip => (Math.Max(PageNumber, 1) - 1) * PageSize;
}
