using SportManager.Application.Brands.Models;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using System.Xml.Linq;

namespace SportManager.Application.Brands.Cammands;

public class UpdateBrandCommand : BrandDto, IRequest<Unit>
{
    public Guid Id { get; set; }
    public void Normalize()
    {
        Name = Name.Normalize();
    }
}

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBrandCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request.Normalize();

        var brand = await _dbContext.Brands
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (brand is null)
            throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());

        var duplicate = await _dbContext.Brands
            .AnyAsync(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != request.Id, cancellationToken);
        if (duplicate)
            throw new ApplicationException("DUPLICATE_NAME");

        brand.Name = request.Name;
        brand.Descriptions = request.Descriptions;
        brand.IsActive = request.IsActive;
        brand.FoundedYear = request.FoundedYear;
        brand.LogoUrl = request.LogoUrl;
        brand.Slug = request.Slug;
        brand.Website = request.Website;
        brand.City = request.City;
        brand.Country = request.Country;
        brand.CountryId = request.CountryId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}