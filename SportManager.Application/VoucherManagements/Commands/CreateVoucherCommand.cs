using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.VoucherManagements.Commands;

public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code.ToUpper(),
            DiscountType = request.DiscountTypeDisplay,
            DiscountValue = request.DiscountValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinOrderValue = request.MinOrderValue,
            MaxUsage = request.RemainingUsage,
            MaxUsagePerUser = request.RemainingUserUsage,
            Description = request.Description
        };

        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync(cancellationToken);
        return voucher.Id;
    }
}
