using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.VoucherUsage)]
public class VoucherUsage : EntityBase<Guid>
{

    [Required]
    public Guid VoucherId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime UsedAt { get; set; }

    public Guid OrderId { get; set; }

    // Navigation
    [ForeignKey(nameof(VoucherId))]
    public Voucher Voucher { get; set; } = default!;
}
