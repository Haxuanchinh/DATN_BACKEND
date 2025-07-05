using SportManager.Application.Common.Interfaces;
using SportManager.Application.Products.Models;
using SportManager.Domain.Entity;
namespace SportManager.Application.Products.Commands.Create;

public class CreateProductCommand : ProductDto, IRequest<CreateProductResponse>
{
    public void Normalize()
    {
        Name = Name.Trim();
        Description = Description!.Trim();
    }
}

public record CreateProductResponse
{
    public Guid Id { get; set; }
}

public class CreateProductCommandHandler(IApplicationDbContext _dbContext)
    : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        request.Normalize();

        //var productId = Guid.NewGuid();
        var existingProduct = await _dbContext.Products
       .AsNoTracking()
       .FirstOrDefaultAsync(p => p.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"Product with name '{request.Name}' already exists.");
        }

        // Check duplicate Variant name + SKU within this product
        var duplicateVariants = request.Variants?
            .GroupBy(v => new { Name = v.Name.Trim().ToLower(), SKU = v.SKU.Trim().ToLower() })
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateVariants != null && duplicateVariants.Any())
        {
            var duplicates = string.Join(", ", duplicateVariants.Select(v => $"(Name: {v.Name}, SKU: {v.SKU})"));
            throw new InvalidOperationException($"Duplicate product variants found: {duplicates}");
        }
        // Create the product without setting Images or navigation properties first
        var product = new Product
        {
            //Id = productId,
            Name = request.Name,
            Description = request.Description,
            BrandId = request.BrandId,
            SupplierId = request.SupplierId,
            //CreatedAt = DateTime.UtcNow
        };

        // Set the Images property manually - this avoids conversion issues
        if (request.Images != null && request.Images.Any())
        {
            // This line might need modification based on how your Product entity handles Images
            // Option 1: If Product.Images is string
            // product.Images = string.Join(",", request.Images);

            // Option 2: If Product.Images is string[]
            product.Images = request.Images;
        }

        // Xử lý Categories
        var productCategories = new List<ProductCategory>();
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
                        //Id = Guid.NewGuid(),
                        Name = cat.Category.Name.Trim(),
                        Description = cat.Category.Description?.Trim(),
                        Logo = cat.Category.Logo
                    };
                    await _dbContext.Categories.AddAsync(newCategory, cancellationToken);
                    categoryId = newCategory.Id;
                }
                else continue;
                productCategories.Add(new ProductCategory
                {
                    //Id = Guid.NewGuid(),
                    //ProductId = productId,
                    CategoryId = categoryId
                });
            }
        }

        // Xử lý Variants
        var variants = new List<ProductVariant>();
        if (request.Variants?.Any() == true)
        {
            foreach (var variant in request.Variants)
            {
                var newVariant = new ProductVariant
                {
                    //Id = Guid.NewGuid(),
                    //ProductId = productId,
                    Name = variant.Name.Trim(),
                    SKU = variant.SKU.Trim(),
                    Unit = variant.Unit.Trim(),
                    Description = variant.Description?.Trim(),
                    Size = variant.Size.Trim(),
                    Color = variant.Color.Trim(),
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity
                };

                // Handle Attribute and Images separately to avoid conversion issues
                if (variant.Attribute != null && variant.Attribute.Length > 0)
                {
                    // Option 1: If ProductVariant.Attribute is string
                    // newVariant.Attribute = string.Join(",", variant.Attribute);

                    // Option 2: If ProductVariant.Attribute is string[]
                    newVariant.Attribute = variant.Attribute;
                }

                if (variant.Images != null && variant.Images.Length > 0)
                {
                    // Option 1: If ProductVariant.Images is string
                    // newVariant.Images = string.Join(",", variant.Images);

                    // Option 2: If ProductVariant.Images is string[]
                    newVariant.Images = variant.Images;
                }

                variants.Add(newVariant);
            }
        }

        // Gán navigation
        product.ProductCategories = productCategories;
        product.Variants = variants;
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new CreateProductResponse { Id = product.Id };
    }
}