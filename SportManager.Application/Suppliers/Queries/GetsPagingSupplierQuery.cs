using SportManager.Application.Common.Interfaces;
using SportManager.Application.Suppliers.Models;
using SportManager.Application.Utilities;

namespace SportManager.Application.Suppliers.Queries;

public class GetsPagingSupplierQueryResponse : SuplierDto
{
    public Guid Id { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class GetsPagingQuery : IRequest<PageResult<GetsPagingSupplierQueryResponse>>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}

public class GetsPagingQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetsPagingQuery, PageResult<GetsPagingSupplierQueryResponse>>
{
    public async Task<PageResult<GetsPagingSupplierQueryResponse>> Handle(GetsPagingQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var keyword = request.Keyword?.Trim();

        var query = dbContext.Suppliers
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(keyword), x =>
                EF.Functions.ILike(x.Name, $"%{keyword}%") ||
                EF.Functions.ILike(x.Description ?? "", $"%{keyword}%"))
            .Select(x => new GetsPagingSupplierQueryResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ContactPhone = x.ContactPhone,
                Address = x.Address,
                City = x.City,
                Region = x.Region,
                PostalCode = x.PostalCode,
                Country = x.Country,
                Phone = x.Phone,
                IsActive = x.IsActive,
                Fax = x.Fax,
                ContactEmail = x.ContactEmail,
                CreatedAt = x.CreatedAt
            })
            .OrderByDescending(x => x.CreatedAt);

        return await PageResult<GetsPagingSupplierQueryResponse>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}
