using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Customer.Models;
using SportManager.Application.Users.Models;

namespace SportManager.Application.Customer.Queries;

public record GetCustomerByIdQuery(Guid Id) : IRequest<GetUserResponse>;

internal class GetByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class GetByIdQueryHandler(IReadOnlyApplicationDbContext applicationDbContext)
    : IRequestHandler<GetCustomerByIdQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var customer = await applicationDbContext.Customers
            .Where(x => x.Id.Equals(request.Id))
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .Include(x => x.ShippingAddresses)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (customer == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        var result = new GetUserResponse
        {
            Id = request.Id,
            Address = customer.Address,
            Age = customer.Age ?? null,
            Gender = customer.Gender,
            Phone = customer.Phone,
            User = new UserView
            {
                Id = customer.User.Id,
                Email = customer.User.Email,
                Username = customer.User.Username
            },
            Roles = customer.User.UserRoles != null
                 ? customer.User.UserRoles.Select(ur => new RoleView
                 {
                     Id = ur.Role.Id,
                     Name = ur.Role.Name
                 }).ToList()
                : new List<RoleView>(),
            ShippingAddresses = customer.ShippingAddresses?.Select(sa => new ShippingAddressView
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
            }).ToList() ?? [],
            CreatedAt = customer.CreatedAt
        };
        return result;
    }
}
