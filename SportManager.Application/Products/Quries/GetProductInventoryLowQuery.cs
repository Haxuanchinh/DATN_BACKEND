using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Products.Quries;

public record GetProductInventoryLowQuery : IRequest<List<ProductInventoryLowDto>>
{
}

public class ProductInventoryLowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    public int Quantity { get; set; }
    public string Price { get; set; }
    public string SupplierName { get; set; }
    public string BrandName { get; set; }
    public ProductParentObj ProductParent { get; set; }
}


public class ProductParentObj
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
public class GetProductInventoryLowQueryHandler : IRequestHandler<GetProductInventoryLowQuery, List<ProductInventoryLowDto>>
{
    private readonly IReadOnlyApplicationDbContext _context;

    public GetProductInventoryLowQueryHandler(IReadOnlyApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductInventoryLowDto>> Handle(GetProductInventoryLowQuery request, CancellationToken cancellationToken)
    {
        return await _context.ProductVariants
                .Where(p => p.StockQuantity < 10)
                .Include(p => p.Product)
                    .ThenInclude(pr => pr.Supplier)
                .Include(p => p.Product)
                    .ThenInclude(pr => pr.Brand)
                .OrderBy(p => p.StockQuantity)
                .Select(p => new ProductInventoryLowDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Quantity = p.StockQuantity,
                    Price = p.Price.ToString("N0") + "đ",
                    SKU = p.SKU,
                    SupplierName = p.Product.Supplier.Name,
                    BrandName = p.Product.Brand.Name,
                    ProductParent = new ProductParentObj
                    {
                        Id = p.Product.Id,
                        Name = p.Product.Name,
                    }
                })
                .ToListAsync(cancellationToken);
    }
}