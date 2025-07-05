using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Band)]
public class Brand : EntityBase<Guid>
{
    public string Name { get; set; }
    public string? Slug { get; set; }
    public string LogoUrl { get; set; }
    public string Website { get; set; }
    public string Country { get; set; }
    public string CountryId { get; set; }
    public string City { get; set; }
    public string? Descriptions { get; set; }
    public bool IsActive { get; set; }
    public int? FoundedYear { get; set; }
    public virtual ICollection<Product>? Products { get; set; }
}
