using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Product)]

public class Product : EntityBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? BrandId { get; set; }
    public virtual Brand? Brand { get; set; }
    //public decimal? BasePrice { get; set; }
    public Guid SupplierId { get; set; }
    public virtual Supplier Supplier { get; set; }
    public virtual ICollection<ProductCategory>? ProductCategories { get; set; }
    public virtual ICollection<ProductVariant>? Variants { get; set; }
    public virtual string[]? Images { get; set; }
    public virtual ICollection<ProductReview>? Reviews { get; set; }
}
