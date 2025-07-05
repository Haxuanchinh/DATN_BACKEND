using SportManager.Application.Common.Interfaces;
using SportManager.Application.Customer.Models;
using SportManager.Application.Products.Models;
using SportManager.Application.Products.Queries;

namespace SportManager.Application.Products.Quries;

public class GetProductByIdQuery : IRequest<ProductPageResponse>
{
    public Guid Id { get; set; }

    public GetProductByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetProductByIdQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetProductByIdQuery, ProductPageResponse>
{
    public async Task<ProductPageResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customerDict = await dbContext.Customers
        .Include(c => c.User)
        .ToDictionaryAsync(c => c.Id, cancellationToken);


        var product = await dbContext.Products
            .AsNoTracking()
            .Include(x => x.ProductCategories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Variants)
            .Include(x => x.Supplier)
            .Include(x => x.Brand)
            .Include(x => x.Reviews)
                .ThenInclude(x => x.Customer)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new ApplicationException("Product not found.");
        }

        return product == null ? null : new ProductPageResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            BrandId = product.BrandId,
            SupplierId = product.SupplierId,
            CreatedAt = product.CreatedAt,
            Images = product.Images ?? Array.Empty<string>(),
            Reviews = product.Reviews?.Select(a => new ProductReviewView
            {
                Id = a.Id,
                Comment = a.Comment,
                CustomerId = a.CustomerId,
                Rating = a.Rating,
                UserName = customerDict.TryGetValue(a.CustomerId, out var customer) ? customer.User.Username : "Unknown"
            }).ToList() ?? new List<ProductReviewView>(),
            ProductCategories = product.ProductCategories?.Select(pc => new ProductCategoryReqDto
            {
                Id = pc.CategoryId,
                Category = pc.Category == null ? null : new CategoryReqdDto
                {
                    Id = pc.Category.Id,
                    Name = pc.Category.Name,
                    Description = pc.Category.Description,
                    Logo = pc.Category.Logo
                }
            }).ToList() ?? new List<ProductCategoryReqDto>(),

            Brand = product.Brand == null ? null : new BrandView
            {
                Id = product.Brand.Id,
                Logo = product.Brand.LogoUrl,
                Name = product.Brand.Name
            },

            Supplier = product.Supplier == null ? null : new Queries.SupplierView
            {
                Id = product.Supplier.Id,
                Name = product.Supplier.Name,
                Address = product.Supplier.Address,
                Email = product.Supplier.ContactEmail,
                Phone = product.Supplier.Phone
            },

            Variants = product.Variants?.Select(v => new ProdVariantReq
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
            }).ToList() ?? new List<ProdVariantReq>()
        };

    }
}
