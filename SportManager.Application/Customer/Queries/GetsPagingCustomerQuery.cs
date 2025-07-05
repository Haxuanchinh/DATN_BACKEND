using SportManager.Application.Common.Interfaces;
using SportManager.Application.Customer.Models;
using SportManager.Application.Users.Models;
using SportManager.Application.Utilities;
using SportManager.Infrastructure.Extensions;
namespace SportManager.Application.Customer.Queries;

public class GetsPagingQuery : IRequest<PageResult<GetUserResponse>>
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 0;
    public int PageSize { get; set; } = 20;
}

public class GetsPagingQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetsPagingQuery, PageResult<GetUserResponse>>
{
    public async Task<PageResult<GetUserResponse>> Handle(GetsPagingQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var keyword = request.Keyword?.Trim();

        var query = dbContext.Customers
            .WhereIf(!string.IsNullOrEmpty(keyword), customer =>
                EF.Functions.ILike(PostgresExtension.Unaccent(customer.User.Username), $"%{keyword}%"))
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
            .Select(customer => new GetUserResponse
            {
                Id = customer.Id,
                Address = customer.Address,
                Phone = customer.Phone,
                Age = customer.Age ?? null,
                Gender = customer.Gender,
                CreatedAt = customer.CreatedAt,
                User = new UserView
                {
                    Id = customer.User.Id,
                    Email = customer.User.Email,
                    Username = customer.User.Username,
                },
                Roles = customer.User.UserRoles != null
                    ? customer.User.UserRoles.Select(ur => new RoleView
                    {
                        Id = ur.Role.Id,
                        Name = ur.Role.Name
                    }).ToList()
                : new List<RoleView>(),

                ShippingAddresses = customer.ShippingAddresses != null && customer.ShippingAddresses.Any() ?
                customer.ShippingAddresses.Select(sa => new Models.ShippingAddressView
                {
                    Id = sa.Id,
                    PostalCode = sa.PostalCode,
                    RecipientName = sa.RecipientName,
                    Ward = sa.Ward,
                    AddressLine = sa.AddressLine,
                    City = sa.City,
                    CityId = sa.CityId,
                    Phone = sa.Phone,
                    Country = sa.Country,
                    CountryId = sa.CountryId,
                    District = sa.District,
                    IsDefault = sa.IsDefault
                }).ToList() : new List<ShippingAddressView>()
            })
            .OrderByDescending(customer => customer.CreatedAt);

        return await PageResult<GetUserResponse>.CreateAsync(query, request.PageNumber, request.PageSize);
    }
}
