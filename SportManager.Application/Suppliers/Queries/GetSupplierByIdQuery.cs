using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.Suppliers.Models;

namespace SportManager.Application.Suppliers.Queries;

public class GetSupplierByIdResponse : SuplierDto
{
    public Guid Id { get; set; }
}
public class GetSupplierByIdQuery : IRequest<GetSupplierByIdResponse>
{
    public Guid Id { get; set; }

    public GetSupplierByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, GetSupplierByIdResponse>
{
    private readonly IReadOnlyApplicationDbContext _dbContext;

    public GetSupplierByIdQueryHandler(IReadOnlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetSupplierByIdResponse> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var supplier = await _dbContext.Suppliers
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetSupplierByIdResponse
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
                Fax = x.Fax,
                ContactEmail = x.ContactEmail,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, "Supplier");
        }

        return supplier;
    }
}
