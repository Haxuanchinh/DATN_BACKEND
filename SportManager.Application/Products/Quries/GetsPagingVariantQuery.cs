using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Products.Quries;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Color { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Unit { get; set; }
    public string? Description { get; set; }
    public List<string>? Images { get; set; }
    public string Size { get; set; }
    public List<string>? Attribute { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaginatedProductVariantsResult
{
    public List<ProductVariantDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class GetAllProductVariantsQuery : IRequest<PaginatedProductVariantsResult>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<Guid> CategoryIds { get; set; } = new();
    public bool IncludeDeleted { get; set; } = false;
}

public class GetAllProductVariantsQueryHandler : IRequestHandler<GetAllProductVariantsQuery, PaginatedProductVariantsResult>
{
    private readonly IReadOnlyApplicationDbContext _dbContext;

    public GetAllProductVariantsQueryHandler(IReadOnlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedProductVariantsResult> Handle(GetAllProductVariantsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ProductVariants.AsQueryable();

        if (!request.IncludeDeleted)
        {
            query = query.Where(x => !x.IsDeleted.HasValue || x.IsDeleted == false);
        }

        // Apply keyword filter if provided
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                x.SKU.ToLower().Contains(keyword) ||
                x.Description != null && x.Description.ToLower().Contains(keyword));
        }

        // Apply category filter if category IDs are provided
        if (request.CategoryIds.Any())
        {
            query = query.Where(x => _dbContext.ProductCategories
                .Any(pc => pc.ProductId == x.ProductId && request.CategoryIds.Contains(pc.CategoryId)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(pv => new ProductVariantDto
            {
                Id = pv.Id,
                ProductId = pv.ProductId,
                Color = pv.Color,
                Name = pv.Name,
                SKU = pv.SKU,
                Unit = pv.Unit,
                Description = pv.Description,
                Images = pv.Images != null ? pv.Images.ToList() : null,
                Size = pv.Size,
                Attribute = pv.Attribute != null ? pv.Attribute.ToList() : null,
                Price = pv.Price,
                StockQuantity = pv.StockQuantity,
                CreatedAt = pv.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedProductVariantsResult
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}