namespace SportManager.Application.Common.Models;

public class PageResult<T>
{
    public PageResult(List<T> items, int totalPages, int totalRecords, int pageNumber, int pageSize)
    {
        TotalPages = totalPages;
        TotalRecords = totalRecords;
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public IEnumerable<T> Items { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public static async Task<PageResult<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip(pageNumber * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);
        var totalRecords = count;
        return new PageResult<T>(items, totalPages, totalRecords, pageNumber, pageSize);
    }

    public static PageResult<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip(pageNumber * pageSize).Take(pageSize).ToList();
        var totalPages = (int)Math.Ceiling(count / (double)pageSize);
        var totalRecords = count;
        return new PageResult<T>(items, totalPages > 0 ? totalPages : 0, totalRecords, pageNumber, pageSize);
    }
}
