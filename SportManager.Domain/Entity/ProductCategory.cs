using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.ProductCategory)]
public class ProductCategory : EntityBase<Guid>
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
}
