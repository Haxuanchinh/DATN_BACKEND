using SportManager.Application.Brands.Models;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Brands.Queries;

public class GetBrandByIdResponse : BrandDto
{
    public Guid Id { get; set; }
}
public class GetBrandByIdQuery : IRequest<GetBrandByIdResponse>
{
    public Guid Id { get; set; }

    public GetBrandByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, GetBrandByIdResponse>
{
    private readonly IReadOnlyApplicationDbContext _dbContext;

    public GetBrandByIdQueryHandler(IReadOnlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetBrandByIdResponse> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var brand = await _dbContext.Brands
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetBrandByIdResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Descriptions = x.Descriptions,
                FoundedYear = x.FoundedYear,
                LogoUrl = x.LogoUrl,
                Website = x.Website,
                City = x.City,
                Country = x.Country,
                CountryId = x.CountryId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (brand is null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());
        }

        return brand;
    }
}