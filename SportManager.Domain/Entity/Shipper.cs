using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Shipper)]
public class Shipper : EntityBase<Guid>
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [StringLength(15)]
    public string PhoneNumber { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    [StringLength(50)]
    public string LicenseNumber { get; set; } // Số bằng lái xe

    [StringLength(20)]
    public string VehicleType { get; set; } // Loại phương tiện

    [StringLength(15)]
    public string VehicleNumber { get; set; } // Biển số xe

    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true; // Có sẵn sàng nhận đơn không

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<ShipperCarrier> ShipperCarriers { get; set; } = new List<ShipperCarrier>();
}

