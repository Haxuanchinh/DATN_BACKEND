using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Products.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.Products.Commands.Update;

public class UpdateProductCommand : ProductDto, IRequest<UpdateCommandResponse>
{
    public Guid Id { get; set; }
    public void Normalize()
    {
        Name = Name.Trim();
        Description = Description?.Trim();
    }
}

public record UpdateCommandResponse
{
    public Guid Id { get; set; }
}
public class UpdateProductCommandHandler(IApplicationDbContext _dbContext)
    : IRequestHandler<UpdateProductCommand, UpdateCommandResponse>
{
    public async Task<UpdateCommandResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        request.Normalize();

        var product = await _dbContext.Products
            .Include(p => p.ProductCategories)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND);
        }

        // Update basic fields
        product.Name = request.Name;
        product.Description = request.Description;
        product.BrandId = request.BrandId;
        product.SupplierId = request.SupplierId;
        product.Images = request.Images ?? [];

        // Update categories
        product.ProductCategories.Clear();
        if (request.ProductCategories?.Any() == true)
        {
            foreach (var cat in request.ProductCategories)
            {
                Guid categoryId;
                if (cat.Id.HasValue)
                {
                    categoryId = cat.Id.Value;
                }
                else if (cat.Category is not null)
                {
                    var newCategory = new Domain.Entity.Category
                    {
                        Name = cat.Category.Name.Trim(),
                        Description = cat.Category.Description?.Trim(),
                        Logo = cat.Category.Logo
                    };
                    await _dbContext.Categories.AddAsync(newCategory, cancellationToken);
                    categoryId = newCategory.Id;
                }
                else continue;

                product.ProductCategories.Add(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId
                });
            }
        }

        // Update variants
        var existingVariants = product.Variants.ToList();
        product.Variants.Clear();

        if (request.Variants?.Any() == true)
        {
            foreach (var variant in request.Variants)
            {
                var existingVariant = existingVariants.FirstOrDefault(v => v.Id == variant.Id);

                if (existingVariant != null)
                {
                    // Update existing variant
                    existingVariant.Name = variant.Name.Trim();
                    existingVariant.SKU = variant.SKU.Trim();
                    existingVariant.Unit = variant.Unit.Trim();
                    existingVariant.Description = variant.Description?.Trim();
                    existingVariant.Size = variant.Size.Trim();
                    existingVariant.Color = variant.Color.Trim();
                    existingVariant.Price = variant.Price;
                    existingVariant.StockQuantity = variant.StockQuantity;
                    existingVariant.Attribute = variant.Attribute ?? [];
                    existingVariant.Images = variant.Images ?? [];

                    product.Variants.Add(existingVariant);
                }
                else
                {
                    // Add new variant
                    var newVariant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Name = variant.Name.Trim(),
                        SKU = variant.SKU.Trim(),
                        Unit = variant.Unit.Trim(),
                        Description = variant.Description?.Trim(),
                        Size = variant.Size.Trim(),
                        Color = variant.Color.Trim(),
                        Price = variant.Price,
                        StockQuantity = variant.StockQuantity,
                        Attribute = variant.Attribute ?? [],
                        Images = variant.Images ?? []
                    };
                    product.Variants.Add(newVariant);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateCommandResponse
        {
            Id = product.Id
        };
    }
}

