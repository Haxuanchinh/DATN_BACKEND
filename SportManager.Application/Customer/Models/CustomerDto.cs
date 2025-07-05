using SportManager.Domain.Entity;

namespace SportManager.Application.Customer.Models;

public class CustomerDto
{
    public Guid? Id { get; set; }
    public Gender? Gender { get; set; }
    public int? Age { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public List<ShippingAddressView>? ShippingAddresses { get; set; } = new List<ShippingAddressView>();
}

public class ShippingAddressView
{
    public Guid? Id { get; set; }
    public string RecipientName { get; set; }
    public string Phone { get; set; }
    public string AddressLine { get; set; }
    public string Country { get; set; }
    public string CountryId { get; set; }
    public string City { get; set; }
    public string CityId { get; set; }
    public string District { get; set; }
    public string Ward { get; set; }
    public string PostalCode { get; set; }
    public bool? IsDefault { get; set; }
}

