using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Shipper_Carrier)]
public class ShipperCarrier : EntityBase<Guid>
{
    public Guid ShipperId { get; set; }
    public Guid CarrierId { get; set; }

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey("ShipperId")]
    public virtual Shipper Shipper { get; set; }

    [ForeignKey("CarrierId")]
    public virtual Carrier Carrier { get; set; }
}

