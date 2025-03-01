namespace Deepin.Infrastructure.Pagination;
public interface IPagination
{
    int Offset { get; }
    int Limit { get; }
    int TotalCount { get; }
}
public interface IPagination<T> : IPagination
{
    IEnumerable<T> Items { get; }
    bool HasMore { get; }
}