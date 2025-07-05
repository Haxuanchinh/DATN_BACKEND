using SportManager.Application.Abstractions;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Customer.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.Customer.Commands.Create;

public class CreateCustomerCommand : CustomerDto, IRequest<CreateCustomerResponse>
{
    public void Normalize()
    {
        Address = Address.Trim();
        Phone = Phone.Trim();
        Email = Email.Trim();
        UserName = UserName.Trim();
    }
}

public class CreateCustomerResponse
{
    public Guid Id { get; set; }
}
public class CreateCustomerValidator : CustomerValidatorBase<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ErrorCode.FIELD_REQUIRED);

        ValidateFormatAndLength();
        ValidateShippingAddresses(false);
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuthService _passwordHasher;

    public CreateCustomerCommandHandler(IApplicationDbContext dbContext, IAuthService passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateCustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request.Normalize();

        var existingUser = await _dbContext.Users
            .Where(u => u.Email.ToLower() == request.Email.ToLower()
                     || u.Username.ToLower() == request.UserName.ToLower())
            .Select(u => new { u.Email, u.Username })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingUser != null)
        {
            if (existingUser.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException("DUPLICATE_EMAIL");

            if (existingUser.Username.Equals(request.UserName, StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException("DUPLICATE_USERNAME");
        }

        var customerRole = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == "Customer" || r.Name == "User", cancellationToken);

        if (customerRole == null)
            throw new ApplicationException("DEFAULT_CUSTOMER_ROLE_NOT_FOUND");

        var user = new User
        {
            Email = request.Email,
            Username = request.UserName,
            Avatar = request?.AvatarUrl,
            PasswordHash = _passwordHasher.HashPassword(request.Password, request.UserName),
            CustomerProfile = new Domain.Entity.Customer
            {
                Gender = (Gender)request.Gender!,
                Age = request.Age,
                Phone = request.Phone!.Trim(),
                Address = request.Address!.Trim(),
                ShippingAddresses = request.ShippingAddresses != null && request.ShippingAddresses.Any() ? request.ShippingAddresses
                .Select(
                    c => new ShippingAddress
                    {
                        IsDefault = c.IsDefault,
                        AddressLine = c.AddressLine,
                        Ward = c.Ward,
                        City = c.City,
                        CityId = c.CityId,
                        Country = c.Country,
                        CountryId = c.CountryId,
                        District = c.District,
                        Phone = c.Phone.Trim(),
                        PostalCode = c.PostalCode,
                        RecipientName = c.RecipientName,
                    }

                    ).ToList() : Array.Empty<ShippingAddress>()
            },
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    RoleId = customerRole.Id,
                }
            }
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new CreateCustomerResponse { Id = user.CustomerProfile.Id };

    }
}


