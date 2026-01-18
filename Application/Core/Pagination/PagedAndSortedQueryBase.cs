namespace Application.Core.Pagination;

public abstract record PagedAndSortedQueryBase<TSortEnum> where TSortEnum : struct, Enum
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TSortEnum? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
