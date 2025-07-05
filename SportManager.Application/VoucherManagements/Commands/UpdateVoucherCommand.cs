using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;

namespace SportManager.Application.VoucherManagements.Commands;

public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FindAsync(new object[] { request.Id }, cancellationToken);

        if (voucher == null)
            throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());

        voucher.Name = request.Name;
        voucher.Code = request.Code.ToUpper();
        voucher.DiscountValue = request.DiscountValue;
        voucher.DiscountType = request.DiscountTypeDisplay;
        voucher.StartDate = request.StartDate;
        voucher.EndDate = request.EndDate;
        voucher.MinOrderValue = request.MinOrderValue;
        voucher.MaxUsage = request.RemainingUsage;
        voucher.MaxUsagePerUser = request.RemainingUserUsage;
        voucher.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

