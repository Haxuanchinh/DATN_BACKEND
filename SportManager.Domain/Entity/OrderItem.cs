using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.OrderItem)]
public class OrderItem : EntityBase<Guid>
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public StateOrder State { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
