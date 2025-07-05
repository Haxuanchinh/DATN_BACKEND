using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Customer)]
public class Customer : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public Gender Gender { get; set; }
    public int? Age { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public virtual ICollection<CustomerSuportTicket> SupportTickets { get; set; } = new List<CustomerSuportTicket>();
}

public enum Gender
{
    Male,
    Female,
    Other
}
