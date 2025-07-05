using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Voucher)]
public class Voucher : EntityBase<Guid>
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    public decimal? MinOrderValue { get; set; }

    public int? MaxUsage { get; set; }

    public int? MaxUsagePerUser { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsPublic { get; set; } = true;

    [MaxLength(50)]
    public string? CreatedBy { get; set; }

    // Navigation
    public virtual ICollection<VoucherUsage> Usages { get; set; } = new List<VoucherUsage>();
    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    public virtual ICollection<ProductVariant>? ProductVariants { get; set; }
}
