using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.VoucherManagements.Models;

namespace SportManager.Application.VoucherManagements.Queries;

public class GetVoucherDetailQuery : IRequest<VoucherDetailDto>
{
    public Guid Id { get; set; }
}

public class GetVoucherDetailQueryHandler : IRequestHandler<GetVoucherDetailQuery, VoucherDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetVoucherDetailQueryHandler(ICurrentUserService currentUserService, IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<VoucherDetailDto> Handle(GetVoucherDetailQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUserService.UserId);
        var roles = _currentUserService.Roles;

        var voucher = await _dbContext.Vouchers
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
            throw new ApplicationException($"Voucher with ID {request.Id} not found");

        // ❗ Nếu không phải Admin hoặc Manager thì cần kiểm tra quyền truy cập
        bool isPrivileged = roles.Contains("Admin") || roles.Contains("Manager");

        if (!voucher.IsPublic && !isPrivileged)
        {
            var hasAccess = await _dbContext.UserVouchers
                .AnyAsync(uv => uv.VoucherId == voucher.Id && uv.UserId == userId, cancellationToken);

            if (!hasAccess)
                throw new ApplicationException("You don't have access to this voucher");
        }

        // Count total usage
        int totalUsage = await _dbContext.VoucherUsages
            .CountAsync(vu => vu.VoucherId == voucher.Id, cancellationToken);

        // Count user usage
        int userUsage = await _dbContext.VoucherUsages
            .CountAsync(vu => vu.VoucherId == voucher.Id && vu.UserId == userId, cancellationToken);

        return new VoucherDetailDto
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
            TotalUsage = totalUsage,
            UserUsage = userUsage,
            IsPublic = voucher.IsPublic,
            CreatedAt = voucher.CreatedAt,
            CreatedBy = voucher.CreatedBy,
            RemainingUsage = voucher.MaxUsage.HasValue ? voucher.MaxUsage.Value - totalUsage : null,
            RemainingUserUsage = voucher.MaxUsagePerUser.HasValue ? voucher.MaxUsagePerUser.Value - userUsage : null
        };
    }
}
