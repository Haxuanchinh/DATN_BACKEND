using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;
using SportManager.Domain.Constants;

namespace SportManager.Application.VoucherManagements.Commands;

public class ValidateVoucherCommand : IRequest<VoucherValidationResultDto>
{
    public string VoucherCode { get; set; } = default!;
    public decimal OrderTotal { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; } // Optional, nếu muốn validate trực tiếp trên đơn hàng cụ thể
}

public class ValidateVoucherCommandHandler : IRequestHandler<ValidateVoucherCommand, VoucherValidationResultDto>
{
    private readonly IApplicationDbContext _dbContext;

    public ValidateVoucherCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VoucherValidationResultDto> Handle(ValidateVoucherCommand request, CancellationToken cancellationToken)
    {
        var result = new VoucherValidationResultDto { IsValid = false };
        decimal orderTotal = request.OrderTotal;

        // Nếu có OrderId, lấy tổng giá trị từ đơn hàng thay vì dùng giá trị truyền vào
        if (request.OrderId.HasValue)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId.Value, cancellationToken);

            if (order == null)
            {
                result.ErrorMessage = "Order not found";
                return result;
            }

            orderTotal = order.CalculateSubTotal();
        }

        // Tìm và xác thực voucher
        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.VoucherCode, cancellationToken);

        if (voucher == null)
        {
            result.ErrorMessage = "Voucher not found";
            return result;
        }

        var currentDate = DateTime.UtcNow;

        // Kiểm tra hiệu lực
        if (currentDate < voucher.StartDate || currentDate > voucher.EndDate)
        {
            result.ErrorMessage = "Voucher is not valid at this time";
            return result;
        }

        // Kiểm tra quyền truy cập 
        if (!voucher.IsPublic)
        {
            var hasAccess = await _dbContext.UserVouchers
                .AnyAsync(uv => uv.VoucherId == voucher.Id && uv.UserId == request.UserId, cancellationToken);

            if (!hasAccess)
            {
                result.ErrorMessage = "You don't have access to this voucher";
                return result;
            }
        }

        // Kiểm tra giá trị đơn hàng tối thiểu
        if (voucher.MinOrderValue.HasValue && orderTotal < voucher.MinOrderValue.Value)
        {
            result.ErrorMessage = $"Minimum order value is {voucher.MinOrderValue.Value}";
            return result;
        }

        // Kiểm tra giới hạn sử dụng
        if (voucher.MaxUsage.HasValue)
        {
            int usageCount = await _dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id, cancellationToken);

            if (usageCount >= voucher.MaxUsage.Value)
            {
                result.ErrorMessage = "Voucher has reached its usage limit";
                return result;
            }
        }

        // Kiểm tra giới hạn sử dụng của người dùng
        if (voucher.MaxUsagePerUser.HasValue)
        {
            int userUsageCount = await _dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == request.UserId, cancellationToken);

            if (userUsageCount >= voucher.MaxUsagePerUser.Value)
            {
                result.ErrorMessage = "You have reached the usage limit for this voucher";
                return result;
            }
        }

        // Tính toán giảm giá
        decimal discountAmount;
        if (voucher.DiscountType == Domain.Constants.DiscountType.Percentage)
        {
            discountAmount = Math.Round(orderTotal * voucher.DiscountValue / 100, 2);
        }
        else
        {
            discountAmount = Math.Min(voucher.DiscountValue, orderTotal);
        }

        // Lấy thống kê sử dụng
        int totalUsage = await _dbContext.VoucherUsages
            .CountAsync(vu => vu.VoucherId == voucher.Id, cancellationToken);

        int userUsage = await _dbContext.VoucherUsages
            .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == request.UserId, cancellationToken);

        // Voucher hợp lệ
        result.IsValid = true;
        result.DiscountAmount = discountAmount;
        result.Voucher = new VoucherDto
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Name = voucher.Name,
            Description = voucher.Description,
            DiscountTypeDisplay = voucher.DiscountType.ToString(),
            DiscountValue = voucher.DiscountValue,
            MinOrderValue = voucher.MinOrderValue,
            StartDate = voucher.StartDate,
            EndDate = voucher.EndDate,
            RemainingUsage = voucher.MaxUsage.HasValue ? voucher.MaxUsage.Value - totalUsage : null,
            RemainingUserUsage = voucher.MaxUsagePerUser.HasValue ? voucher.MaxUsagePerUser.Value - userUsage : null
        };

        return result;
    }
}