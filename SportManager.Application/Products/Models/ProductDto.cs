using SportManager.Application.Customer.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.Products.Models;

public class ProductDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? BrandId { get; set; }
    public Guid SupplierId { get; set; }
    public virtual ICollection<ProductCategoryReqDto> ProductCategories { get; set; }
    public virtual ICollection<ProdVariantReq> Variants { get; set; }
    public virtual string[]? Images { get; set; }
}

public class ProductReviewView
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid CustomerId { get; set; }
    public string UserName { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
}


public class ProductCategoryReqDto
{
    public Guid? Id { get; set; }
    public CategoryReqdDto? Category { get; set; }
}


public class CategoryReqdDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
}

public class ProdVariantReq
{
    public Guid? Id { get; set; }
    public string Color { get; set; }
    public string Name { get; set; }
    public string SKU { get; set; }
    public string Unit { get; set; }
    public string? Description { get; set; }
    public string[]? Images { get; set; }
    public string Size { get; set; }
    public string[]? Attribute { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class SupplierView
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string Phone { get; set; }
    public bool IsActive { get; set; }
    public string? Fax { get; set; }
    public string ContactEmail { get; set; }
}

public record ProductReviewCommentDto(
    Guid Id,
    Guid? ParentCommentId,
    Guid CustomerId,
    string UserName,
    string Content,
    DateTime CreatedAt,
    List<ProductReviewCommentDto> Replies
);

public record ProductReviewDto(
    Guid Id,
    Guid ProductId,
    Guid? VariantId,
    Guid CustomerId,
    string UserName,
    int Rating,
    string Comment,
    bool IsVerifiedPurchase,
    List<string>? Images,
    List<ProductReviewCommentDto> Comments
);
