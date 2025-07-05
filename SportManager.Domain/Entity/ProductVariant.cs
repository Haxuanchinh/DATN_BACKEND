using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.ProductVariant)]
public class ProductVariant : EntityBase<Guid>
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
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
    public virtual Voucher? Voucher { get; set; }
}
