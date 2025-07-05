using SportManager.Application.Common.Interfaces;
using SportManager.Application.Utilities;
using SportManager.Application.VoucherManagements.Models;

namespace SportManager.Application.VoucherManagements.Queries;

public class GetAllVouchersQuery : IRequest<PageResult<VoucherDto>>
{
    public string? Keyword { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}

public class GetAllVouchersQueryHandler : IRequestHandler<GetAllVouchersQuery, PageResult<VoucherDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllVouchersQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<VoucherDto>> Handle(GetAllVouchersQuery request, CancellationToken cancellationToken)
    {
        var keyword = request.Keyword?.Trim();

        var query = _dbContext.Vouchers
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(keyword), x =>
                EF.Functions.ILike(x.Name, $"%{keyword}%") ||
                EF.Functions.ILike(x.Description ?? "", $"%{keyword}%"))
            .WhereIf(request.StartDateFrom.HasValue, x => x.StartDate >= request.StartDateFrom)
            .WhereIf(request.EndDateTo.HasValue, x => x.EndDate <= request.EndDateTo)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                Description = v.Description,
                DiscountTypeDisplay = v.DiscountType.ToString(),
                DiscountValue = v.DiscountValue,
                MinOrderValue = v.MinOrderValue,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                RemainingUsage = v.MaxUsage,
                RemainingUserUsage = v.MaxUsagePerUser,
                CreatedAt = v.CreatedAt,
            })
            .OrderByDescending(x => x.CreatedAt);

        return await PageResult<VoucherDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}
