using SportManager.Application.Customer.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.Users.Models;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public Gender Gender { get; set; }
    public int? Age { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string? AvatarUrl { get; set; }
    public UserView User { get; set; }
    public List<ShippingAddressView>? ShippingAddresses { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RoleView>? Roles { get; set; }
}
public class UserView
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}

public class RoleView
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
