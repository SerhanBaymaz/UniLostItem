namespace Application.Core.Pagination;

public interface IPaginatedList
{
    int TotalCount { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
    object? GetItems();
}
