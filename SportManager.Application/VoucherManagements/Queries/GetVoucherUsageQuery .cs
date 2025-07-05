using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;
using SportManager.Domain.Constants;

namespace SportManager.Application.VoucherManagements.Queries;

public class GetVoucherUsageQuery : IRequest<IEnumerable<VoucherUsageDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? VoucherId { get; set; }
    public Guid? UserId { get; set; }
}

public class GetVoucherUsageQueryHandler : IRequestHandler<GetVoucherUsageQuery, IEnumerable<VoucherUsageDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetVoucherUsageQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<VoucherUsageDto>> Handle(GetVoucherUsageQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.VoucherUsages.AsQueryable();

        // Apply filters
        if (request.StartDate.HasValue)
            query = query.Where(vu => vu.UsedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(vu => vu.UsedAt <= request.EndDate.Value);

        if (request.VoucherId.HasValue)
            query = query.Where(vu => vu.VoucherId == request.VoucherId.Value);

        if (request.UserId.HasValue)
            query = query.Where(vu => vu.UserId == request.UserId.Value);

        // Join with related data
        var result = await query
            .Join(_dbContext.Vouchers,
                vu => vu.VoucherId,
                v => v.Id,
                (vu, v) => new { VoucherUsage = vu, Voucher = v })
            .Join(_dbContext.Users,
                x => x.VoucherUsage.UserId,
                u => u.Id,
                (x, u) => new { x.VoucherUsage, x.Voucher, User = u })
            .Join(_dbContext.Orders,
                x => x.VoucherUsage.OrderId,
                o => o.Id,
                (x, o) => new VoucherUsageDto
                {
                    Id = x.VoucherUsage.Id,
                    VoucherId = x.Voucher.Id,
                    VoucherCode = x.Voucher.Code,
                    VoucherName = x.Voucher.Name,
                    UserId = x.User.Id,
                    UserName = x.User.Username,
                    OrderId = o.Id,
                    DiscountAmount = CalculateDiscountAmount(x.Voucher.DiscountType, x.Voucher.DiscountValue, o.Total),
                    UsedAt = x.VoucherUsage.UsedAt
                })
            .ToListAsync(cancellationToken);

        return result;
    }

    private decimal CalculateDiscountAmount(DiscountType discountType, decimal discountValue, decimal orderTotal)
    {
        if (discountType == DiscountType.Percentage)
        {
            return orderTotal * discountValue / 100;
        }
        return discountValue;
    }
}

