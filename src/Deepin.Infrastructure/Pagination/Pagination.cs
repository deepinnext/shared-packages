namespace Deepin.Infrastructure.Pagination;
public class Pagination<T> : IPagination<T>
{
    public IEnumerable<T> Items { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public Pagination()
    {
        Items = [];
    }
    public Pagination(IQueryable<T> source, int offset, int limit)
    {
        int totalCount = source.Count();
        TotalCount = totalCount;
        Limit = limit;
        Offset = offset;
        HasMore = offset + limit < totalCount;
        Items = source.Skip(offset).Take(limit);
    }
    public Pagination(IEnumerable<T> source, int offset, int limit, int totalCount)
    {
        TotalCount = totalCount;
        Limit = limit;
        Offset = offset;
        HasMore = offset + limit < totalCount;
        Items = source;
    }
}
