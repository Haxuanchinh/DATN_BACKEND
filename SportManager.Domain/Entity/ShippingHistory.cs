using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Shipping_History)]
public class ShippingHistory : EntityBase<Guid>
{
    public Guid OrderId { get; set; }

    public ShippingStatus Status { get; set; }

    [StringLength(200)]
    public string Description { get; set; }

    [StringLength(200)]
    public string Location { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? UpdatedBy { get; set; } // Ai cập nhật trạng thái này

    // Navigation property
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
}
