using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Carrier)]
public class Carrier : EntityBase<Guid>
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } // Tên đơn vị vận chuyển (VD: Giao Hàng Nhanh, Viettel Post...)

    [StringLength(20)]
    public string Code { get; set; } // Mã đơn vị vận chuyển

    [StringLength(15)]
    public string PhoneNumber { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "decimal(10,2)")]
    public decimal BaseShippingFee { get; set; } // Phí vận chuyển cơ bản

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ShipperCarrier> ShipperCarriers { get; set; } = new List<ShipperCarrier>();
}
