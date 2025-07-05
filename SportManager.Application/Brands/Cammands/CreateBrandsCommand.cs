using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Brands.Models;
using SportManager.Application.Suppliers.Models;

namespace SportManager.Application.Brand.Commands.Create;

public class CreateBrandCommand : BrandDto, IRequest<CreateBrandResponse>
{
    public void Normalize()
    {
        Name = Name.Normalize();
    }
}

public class CreateBrandResponse
{
    public Guid Id { get; set; }
}
public class CreateBrandValidator : AbstractValidator<CreateBrandCommand>
{
}

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, CreateBrandResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateBrandCommandHandler(IApplicationDbContext dbContext, IAuthService passwordHasher)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateBrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request.Normalize();

        var existingBrand = await _dbContext.Brands
            .Where(u => u.Name.ToLower() == request.Name.ToLower())
            .Select(u => new { u.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBrand != null)
        {
            if (existingBrand.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
                throw new ApplicationException("DUPLICATE_NAME");
        }

        var brand = new Domain.Entity.Brand
        {
            Name = request.Name,
            Descriptions = request.Descriptions,
            IsActive = request.IsActive,
            FoundedYear = request.FoundedYear,
            LogoUrl = request.LogoUrl,
            Slug = request.Slug,
            Website = request.Website,
            City = request.City,
            Country = request.Country,
            CountryId = request.CountryId
        };

        _dbContext.Brands.Add(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new CreateBrandResponse { Id = brand.Id };

    }
}


