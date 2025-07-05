using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Constants;
using SportManager.Domain.Entity;

namespace SportManager.Application.Common.Exception;

//public static class OrderExtensions
//{
//    /// <summary>
//    /// Tính tổng giá trị của đơn hàng dựa trên các OrderItem
//    /// </summary>
//    public static decimal CalculateSubTotal(this Order order)
//    {
//        if (order.OrderItems == null || !order.OrderItems.Any())
//            return 0;

//        return order.OrderItems.Sum(item => item.Quantity * item.UnitPrice);
//    }

//    /// <summary>
//    /// Tính số tiền giảm giá dựa trên voucher và đơn hàng
//    /// </summary>
//    public static decimal CalculateDiscountAmount(
//        this Order order,
//        Voucher voucher,
//        IEnumerable<Product> products = null)
//    {
//        var subTotal = order.CalculateSubTotal();

//        if (voucher.DiscountType == DiscountType.Percentage)
//        {
//            return Math.Round(subTotal * voucher.DiscountValue / 100, 2);
//        }
//        else // Fixed amount
//        {
//            return Math.Min(voucher.DiscountValue, subTotal);
//        }
//    }
//}

public static class OrderExtensions
{
    public static decimal CalculateSubTotal(this Order order)
    {
        return order.OrderItems.Sum(item => item.TotalPrice);
    }

    public static decimal CalculateDiscountAmount(this Order order, Voucher voucher)
    {
        decimal subTotal = order.CalculateSubTotal();

        switch (voucher.DiscountType)
        {
            case (Domain.Constants.DiscountType)DiscountType.Percentage:
                var percentageDiscount = subTotal * (voucher.DiscountValue / 100);
                // Có thể thêm max discount amount nếu cần
                return Math.Round(percentageDiscount, 2);

            case (Domain.Constants.DiscountType)DiscountType.FixedAmount:
                // Discount không được vượt quá tổng tiền đơn hàng
                return Math.Min(voucher.DiscountValue, subTotal);

            default:
                return 0;
        }
    }
}

// Enum cho discount type
public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2
}

// Cải tiến ValidateVoucherCommand
public class ValidateVoucherCommand : IRequest<VoucherValidationResult>
{
    public string VoucherCode { get; set; } = default!;
    public decimal OrderTotal { get; set; }
    public Guid UserId { get; set; }
}

public class VoucherValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Voucher? Voucher { get; set; }
}

public class ValidateVoucherCommandHandler : IRequestHandler<ValidateVoucherCommand, VoucherValidationResult>
{
    private readonly IApplicationDbContext _dbContext;

    public ValidateVoucherCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VoucherValidationResult> Handle(ValidateVoucherCommand request, CancellationToken cancellationToken)
    {
        // Tìm voucher
        var voucher = await _dbContext.Vouchers
            .Include(v => v.Usages)
            .Include(v => v.UserVouchers)
            .FirstOrDefaultAsync(v => v.Code == request.VoucherCode, cancellationToken);

        if (voucher == null)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorMessage = "Mã voucher không tồn tại"
            };
        }

        // Kiểm tra thời gian hiệu lực
        var currentDate = DateTime.UtcNow;
        if (currentDate < voucher.StartDate)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorMessage = "Voucher chưa có hiệu lực"
            };
        }

        if (currentDate > voucher.EndDate)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorMessage = "Voucher đã hết hạn"
            };
        }

        // Kiểm tra giá trị đơn hàng tối thiểu
        if (voucher.MinOrderValue.HasValue && request.OrderTotal < voucher.MinOrderValue.Value)
        {
            return new VoucherValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Đơn hàng phải có giá trị tối thiểu {voucher.MinOrderValue.Value:C}"
            };
        }

        // Kiểm tra số lần sử dụng tổng
        if (voucher.MaxUsage.HasValue)
        {
            var totalUsageCount = voucher.Usages.Count;
            if (totalUsageCount >= voucher.MaxUsage.Value)
            {
                return new VoucherValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Voucher đã hết lượt sử dụng"
                };
            }
        }

        // Kiểm tra số lần sử dụng per user
        if (voucher.MaxUsagePerUser.HasValue)
        {
            var userUsageCount = voucher.Usages.Count(u => u.UserId == request.UserId);
            if (userUsageCount >= voucher.MaxUsagePerUser.Value)
            {
                return new VoucherValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Bạn đã sử dụng hết lượt áp dụng voucher này"
                };
            }
        }

        // Kiểm tra voucher có phải là public hay chỉ dành cho user cụ thể
        if (!voucher.IsPublic)
        {
            var hasUserVoucher = voucher.UserVouchers.Any(uv => uv.UserId == request.UserId);
            if (!hasUserVoucher)
            {
                return new VoucherValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Bạn không có quyền sử dụng voucher này"
                };
            }
        }

        return new VoucherValidationResult
        {
            IsValid = true,
            Voucher = voucher
        };
    }
}

// Model để trả về kết quả apply voucher
public class ApplyVoucherResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NewTotal { get; set; }
}
