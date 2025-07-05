using SportManager.Application.Common.Interfaces;
using SportManager.Application.Products.Models;

namespace SportManager.Application.Products.Queries;

public class ProductPageResponse : ProductDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ProductReviewView> Reviews { get; set; } = new List<ProductReviewView>();
    public SupplierView Supplier { get; set; } = null!; // Enforced by null check in mapping
    public BrandView? Brand { get; set; }
}

public record SupplierView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string Email { get; set; } = null!;
    public string Address { get; set; } = null!;
}

public record BrandView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Logo { get; set; }
}

public class GetsPagingQuery : IRequest<PageResult<ProductPageResponse>>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<Guid> CategoryIds { get; set; } = new();
}

public class GetsPagingQueryHandler : IRequestHandler<GetsPagingQuery, PageResult<ProductPageResponse>>
{
    private readonly IReadOnlyApplicationDbContext _dbContext;
    private const int MaxPageSize = 100;

    public GetsPagingQueryHandler(IReadOnlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<ProductPageResponse>> Handle(GetsPagingQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keyword = request.Keyword?.Trim();

        var baseQuery = _dbContext.Products.AsNoTracking();

        // Apply filters
        if (request.CategoryIds.Count > 0)
        {
            baseQuery = baseQuery.Where(product =>
                product.ProductCategories.Any(pc => request.CategoryIds.Contains(pc.CategoryId)));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            baseQuery = baseQuery.Where(product =>
                EF.Functions.ILike(product.Name, $"%{keyword}%"));
        }

        // Projection query - avoids loading unnecessary data
        var projectedQuery = baseQuery
            .OrderByDescending(product => product.CreatedAt)
            .Select(product => new ProductPageResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                BrandId = product.BrandId,
                SupplierId = product.SupplierId,
                CreatedAt = product.CreatedAt,
                Images = product.Images ?? Array.Empty<string>(),

                ProductCategories = product.ProductCategories.Select(pc => new ProductCategoryReqDto
                {
                    Id = pc.CategoryId,
                    Category = new CategoryReqdDto
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name,
                        Description = pc.Category.Description,
                        Logo = pc.Category.Logo
                    }
                }).ToList(),

                Brand = product.Brand == null ? null : new BrandView
                {
                    Id = product.Brand.Id,
                    Logo = product.Brand.LogoUrl,
                    Name = product.Brand.Name
                },

                Supplier = new SupplierView
                {
                    Id = product.Supplier.Id,
                    Name = product.Supplier.Name,
                    Address = product.Supplier.Address ?? string.Empty,
                    Email = product.Supplier.ContactEmail ?? string.Empty,
                    Phone = product.Supplier.Phone
                },

                Reviews = product.Reviews.Select(rv => new ProductReviewView
                {
                    ProductId = rv.ProductId,
                    CustomerId = rv.CustomerId,
                    Comment = rv.Comment ?? string.Empty,
                    UserName = rv.Customer != null && rv.Customer.User != null
                        ? rv.Customer.User.Username
                        : string.Empty,
                    Rating = rv.Rating
                }).ToList(),

                Variants = product.Variants.Select(v => new ProdVariantReq
                {
                    Id = v.Id,
                    Name = v.Name,
                    Color = v.Color,
                    SKU = v.SKU,
                    Unit = v.Unit,
                    Description = v.Description,
                    Size = v.Size,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Images = v.Images ?? Array.Empty<string>(),
                    Attribute = v.Attribute ?? Array.Empty<string>()
                }).ToList()
            });

        return await PageResult<ProductPageResponse>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}