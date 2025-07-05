using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Supplier)]
public class Supplier : EntityBase<Guid>
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
    public ICollection<Product>? Products { get; set; }
}
