using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;

namespace SportManager.Application.VoucherManagements.Queries;

public class GetAvailableVouchersQuery : IRequest<IEnumerable<VoucherDto>>
{
}

public class GetAvailableVouchersQueryHandler : IRequestHandler<GetAvailableVouchersQuery, IEnumerable<VoucherDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableVouchersQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<VoucherDto>> Handle(GetAvailableVouchersQuery req, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUserService.UserId);
        var currentDate = DateTime.UtcNow;

        // Get public vouchers
        var publicVouchersQuery = _dbContext.Vouchers
            .Where(v => v.IsPublic)
            .Where(v => v.StartDate <= currentDate && v.EndDate >= currentDate);

        // Get user assigned vouchers
        var userVouchersQuery = _dbContext.Vouchers
            .Join(_dbContext.UserVouchers,
                v => v.Id,
                uv => uv.VoucherId,
                (v, uv) => new { Voucher = v, UserVoucher = uv })
            .Where(x => x.UserVoucher.UserId == userId)
            .Where(x => x.Voucher.StartDate <= currentDate && x.Voucher.EndDate >= currentDate)
            .Select(x => x.Voucher);

        // Combine queries
        var combinedVouchers = await publicVouchersQuery
            .Union(userVouchersQuery)
            .ToListAsync(cancellationToken);

        var result = new List<VoucherDto>();

        foreach (var voucher in combinedVouchers)
        {
            // Check total usage
            int usageCount = await _dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id, cancellationToken);

            // Check user usage
            int userUsageCount = await _dbContext.VoucherUsages
                .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == userId, cancellationToken);

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
