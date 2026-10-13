namespace StudentManagement.Application.Common;

public enum SortDirection
{
    Ascending,
    Descending,
}

/// <summary>Paging and sort direction shared by list queries. Out-of-range values fall back to defaults.</summary>
public abstract record PagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    private readonly int _page = 1;
    private readonly int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
    }

    public SortDirection Direction { get; init; }

    public int Skip => (Page - 1) * PageSize;
}
