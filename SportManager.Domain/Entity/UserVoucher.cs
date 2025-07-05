using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.UserVoucher)]
public class UserVoucher : EntityBase<Guid>
{
    [Required]
    public Guid VoucherId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(VoucherId))]
    public Voucher Voucher { get; set; } = default!;
}
