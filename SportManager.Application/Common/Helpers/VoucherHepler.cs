using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Constants;

namespace SportManager.Application.Common.Helpers;

public static class VoucherHelper
{
    public static decimal CalculateDiscountAmount(DiscountType discountType, decimal discountValue, decimal orderTotal)
    {
        switch (discountType)
        {
            case DiscountType.Percentage:
                return Math.Round(orderTotal * discountValue / 100, 2);
            case DiscountType.FixedAmount:
                return Math.Min(discountValue, orderTotal); // Cap at order total
            default:
                throw new ArgumentException($"Unsupported discount type: {discountType}");
        }
    }

    public static async Task<bool> IsVoucherValidAsync(
        IApplicationDbContext dbContext,
        string voucherCode,
        Guid userId,
        decimal orderTotal,
        CancellationToken cancellationToken = default)
    {
        // Find voucher by code
        var voucher = await dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.Code == voucherCode, cancellationToken);

        if (voucher == null)
            return false;

        var currentDate = DateTime.UtcNow;

        // Check validity period
        if (currentDate < voucher.StartDate || currentDate > voucher.EndDate)
            return false;

        // Check if user has access
        if (!voucher.IsPublic)
        {
            var hasAccess = await dbContext.UserVouchers
                .AnyAsync(uv => uv.VoucherId == voucher.Id && uv.UserId == userId, cancellationToken);

            if (!hasAccess)
                return false;
        }

        // Check minimum order value
        if (voucher.MinOrderValue.HasValue && orderTotal < voucher.MinOrderValue.Value)
            return false;

        // Check total usage limit
        if (voucher.MaxUsage.HasValue)
        {
            int usageCount = await dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id, cancellationToken);

            if (usageCount >= voucher.MaxUsage.Value)
                return false;
        }

        // Check per-user limit
        if (voucher.MaxUsagePerUser.HasValue)
        {
            int userUsageCount = await dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == userId, cancellationToken);

            if (userUsageCount >= voucher.MaxUsagePerUser.Value)
                return false;
        }

        return true;
    }
}
