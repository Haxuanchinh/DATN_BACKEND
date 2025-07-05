namespace SportManager.Application.Brands.Models;

public class BrandDto
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
}
