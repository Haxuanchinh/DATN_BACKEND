using SportManager.Domain.Entity;

namespace SportManager.Application.Categroies.Models;

public class CategoryDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Logo { get; set; }
    // public ICollection<ProductCategory> ProductCategories { get; set; }
}
