using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;

namespace SportManager.Application.VoucherManagements.Queries;

public class GetUserVouchersQuery : IRequest<IEnumerable<VoucherDto>>
{
    public Guid UserId { get; set; }
}

public class GetUserVouchersQueryHandler : IRequestHandler<GetUserVouchersQuery, IEnumerable<VoucherDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserVouchersQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<VoucherDto>> Handle(GetUserVouchersQuery request, CancellationToken cancellationToken)
    {
        var userVouchers = await _dbContext.UserVouchers
            .Where(uv => uv.UserId == request.UserId)
            .Join(_dbContext.Vouchers,
                uv => uv.VoucherId,
                v => v.Id,
                (uv, v) => v)
            .ToListAsync(cancellationToken);

        var result = new List<VoucherDto>();
        var currentDate = DateTime.UtcNow;

        foreach (var voucher in userVouchers)
        {
            // Skip expired vouchers
            if (voucher.EndDate < currentDate)
                continue;

            // Count total usage
            int usageCount = await _dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id, cancellationToken);

            // Count user usage
            int userUsageCount = await _dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == request.UserId, cancellationToken);

            // Skip if max usage is reached
            if (voucher.MaxUsage.HasValue && usageCount >= voucher.MaxUsage.Value)
                continue;

            // Skip if max usage per user is reached
            if (voucher.MaxUsagePerUser.HasValue && userUsageCount >= voucher.MaxUsagePerUser.Value)
                continue;

            result.Add(new VoucherDto
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
                RemainingUsage = voucher.MaxUsage.HasValue ? voucher.MaxUsage.Value - usageCount : null,
                RemainingUserUsage = voucher.MaxUsagePerUser.HasValue ? voucher.MaxUsagePerUser.Value - userUsageCount : null
            });
        }

        return result;
    }
}
