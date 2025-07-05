using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Category)]
public class Category : EntityBase<Guid>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; }
}
