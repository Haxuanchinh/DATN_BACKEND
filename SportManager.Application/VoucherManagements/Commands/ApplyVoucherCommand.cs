using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;
using SportManager.Domain.Constants;
using SportManager.Domain.Entity;
using ApplyVoucherResultDto = SportManager.Application.VoucherManagements.Models.ApplyVoucherResultDto;

namespace SportManager.Application.VoucherManagements.Commands;

public class ApplyVoucherCommand : IRequest<ApplyVoucherResultDto>
{
    public string VoucherCode { get; set; } = default!;
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}

public class ApplyVoucherCommandHandler : IRequestHandler<ApplyVoucherCommand, ApplyVoucherResultDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public ApplyVoucherCommandHandler(IApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<ApplyVoucherResultDto> Handle(ApplyVoucherCommand request, CancellationToken cancellationToken)
    {
        // Tìm đơn hàng và kiểm tra xem nó có thuộc về người dùng hiện tại không
        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return new ApplyVoucherResultDto
            {
                Success = false,
                ErrorMessage = "Order not found"
            };
        }

        // Có thể thêm kiểm tra xem đơn hàng có thuộc về user hiện tại không
        if (order.CustomerId != request.UserId)
        {
            return new ApplyVoucherResultDto
            {
                Success = false,
                ErrorMessage = "You don't have permission to apply voucher to this order"
            };
        }

        // Tính tổng giá trị đơn hàng từ các OrderItem
        decimal orderTotal = order.CalculateSubTotal();

        // Tìm và xác thực voucher
        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.VoucherCode, cancellationToken);

        if (voucher == null)
        {
            return new ApplyVoucherResultDto
            {
                Success = false,
                ErrorMessage = "Voucher not found"
            };
        }

        // Kiểm tra tính hợp lệ của voucher
        var validationResult = await _mediator.Send(new ValidateVoucherCommand
        {
            VoucherCode = request.VoucherCode,
            OrderTotal = orderTotal,
            UserId = request.UserId
        }, cancellationToken);

        if (!validationResult.IsValid)
        {
            return new ApplyVoucherResultDto
            {
                Success = false,
                ErrorMessage = validationResult.ErrorMessage
            };
        }

        // Tính toán giảm giá
        decimal discountAmount = order.CalculateDiscountAmount(voucher);

        // Cập nhật đơn hàng với mã giảm giá
        order.VoucherId = voucher.Id;
        order.DiscountAmount = discountAmount;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Thêm bản ghi sử dụng voucher
        var voucherUsage = new VoucherUsage
        {
            Id = Guid.NewGuid(),
            VoucherId = voucher.Id,
            UserId = request.UserId,
            OrderId = request.OrderId,
            UsedAt = DateTime.UtcNow
        };

        _dbContext.VoucherUsages.Add(voucherUsage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApplyVoucherResultDto
        {
            Success = true,
            DiscountAmount = discountAmount
        };
    }
}