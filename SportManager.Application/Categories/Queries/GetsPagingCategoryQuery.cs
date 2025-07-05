using SportManager.Application.Categroies.Models;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Utilities;

namespace SportManager.Application.Categories.Queries;

public class GetsPagingCategoryQueryResponse : CategoryDto
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class GetsPagingQuery : IRequest<PageResult<GetsPagingCategoryQueryResponse>>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}
public class GetsPagingQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetsPagingQuery, PageResult<GetsPagingCategoryQueryResponse>>
{
    public async Task<PageResult<GetsPagingCategoryQueryResponse>> Handle(GetsPagingQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var keyword = request.Keyword?.Trim();
        // Using AsNoTracking for better performance in read-only queries
        var query = dbContext.Categories
            .WhereIf(!string.IsNullOrEmpty(keyword), product =>
                EF.Functions.ILike(product.Name, $"%{keyword!}%"))
            .Select(product => new GetsPagingCategoryQueryResponse
            {
                Id = product.Id,
                Description = product.Description,
                Logo = product.Logo,
                Name = product.Name,
                CreatedAt = product.CreatedAt,
            })
            .OrderByDescending(product => product.CreatedAt);
        return await PageResult<GetsPagingCategoryQueryResponse>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}