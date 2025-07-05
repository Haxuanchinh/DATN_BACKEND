namespace SportManager.Application.ShippingAddresses.Models;

public class ShippingAddressDtoView
{
    public Guid? CustomerId { get; set; }
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

public class ShippingAddressResponse : ShippingAddressDtoView
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

