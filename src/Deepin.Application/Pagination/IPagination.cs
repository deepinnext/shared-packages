namespace Deepin.Application.Pagination;
public interface IPagination
{
    int Offset { get; }
    int Limit { get; }
    int TotalPages { get; }
    int TotalCount { get; }
}
public interface IPagination<T> : IPagination
{
    IEnumerable<T> Items { get; }
}