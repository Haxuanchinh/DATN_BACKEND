using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.CartItem)]
public class CartItem : EntityBase<Guid>
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }
    public int Quantity { get; set; }
    public int Unit { get; set; }
    public int UnitPrice { get; set; }
}

// 1 : cái , 2 hộp, 3 đôi, 4 