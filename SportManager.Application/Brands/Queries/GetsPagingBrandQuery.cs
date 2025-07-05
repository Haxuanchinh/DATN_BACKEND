using SportManager.Application.Brands.Models;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Products.Quries;
using SportManager.Application.Utilities;

namespace SportManager.Application.Brands.Queries;

public class GetsPagingBrandQueryResponse : BrandDto
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class GetsPagingQuery : IRequest<PageResult<GetsPagingBrandQueryResponse>>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}
public class GetsPagingQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetsPagingQuery, PageResult<GetsPagingBrandQueryResponse>>
{
    public async Task<PageResult<GetsPagingBrandQueryResponse>> Handle(GetsPagingQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var keyword = request.Keyword?.Trim();
        // Using AsNoTracking for better performance in read-only queries
        var query = dbContext.Brands
            .WhereIf(!string.IsNullOrEmpty(keyword), product =>
                EF.Functions.ILike(product.Name, $"%{keyword!}%"))
            .Select(product => new GetsPagingBrandQueryResponse
            {
                Id = product.Id,
                City = product.City,
                Country = product.Country,
                CountryId = product.CountryId,
                CreatedAt = product.CreatedAt,
                Descriptions = product.Descriptions,
                FoundedYear = product.FoundedYear,
                IsActive = product.IsActive,
                LogoUrl = product.LogoUrl,
                Name = product.Name,
                Slug = product.Slug,
                Website = product.Website,
            })
            .OrderByDescending(product => product.CreatedAt);
        return await PageResult<GetsPagingBrandQueryResponse>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}