using SportManager.Application.Carts.Commands.Create;
using SportManager.Domain.Entity;

namespace SportManager.Application.Carts.Models;

public class AddToCartCommand : IRequest<AddToCartResponse>
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public ProductVariantView? ProductVariant { get; set; }
}

public class ProductVariantView
{
    public Guid Id { get; set; }
    //public Product Product { get; set; }
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