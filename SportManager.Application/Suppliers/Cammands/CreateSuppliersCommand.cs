using SportManager.Application.Suppliers.Models;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Supplier.Commands.Create;

public class CreateSupplierCommand : SuplierDto, IRequest<CreateSupplierResponse>
{
    public void Normalize()
    {
        Name = Name?.Trim();
    }
}

public class CreateSupplierResponse
{
    public Guid Id { get; set; }
}

public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nhà cung cấp là bắt buộc");
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
    }
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, CreateSupplierResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSupplierCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateSupplierResponse> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request.Normalize();

        var existingSupplier = await _dbContext.Suppliers
            .AnyAsync(u => u.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingSupplier)
        {
            throw new ApplicationException("DUPLICATE_NAME");
        }

        var supplier = new Domain.Entity.Supplier
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
            City = request.City,
            Region = request.Region,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Phone = request.Phone,
            Fax = request.Fax,
            ContactEmail = request.ContactEmail
        };

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateSupplierResponse { Id = supplier.Id };
    }
}

public class UpdateSupplierCommand : SuplierDto, IRequest<Unit>
{
    public Guid Id { get; set; }

    public void Normalize()
    {
        Name = Name?.Trim();
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSupplierCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        request.Normalize();

        var supplier = await _dbContext.Suppliers.FindAsync(new object[] { request.Id }, cancellationToken);

        if (supplier == null)
            throw new KeyNotFoundException("SUPPLIER_NOT_FOUND");

        supplier.Name = request.Name;
        supplier.Description = request.Description;
        supplier.IsActive = request.IsActive;
        supplier.ContactPhone = request.ContactPhone;
        supplier.Address = request.Address;
        supplier.City = request.City;
        supplier.Region = request.Region;
        supplier.PostalCode = request.PostalCode;
        supplier.Country = request.Country;
        supplier.Phone = request.Phone;
        supplier.Fax = request.Fax;
        supplier.ContactEmail = request.ContactEmail;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public record DeleteSupplierCommand(Guid Id) : IRequest<Unit>
{
}

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteSupplierCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var supplier = await _dbContext.Suppliers.FindAsync(new object[] { request.Id }, cancellationToken);
        if (supplier == null)
            throw new KeyNotFoundException("SUPPLIER_NOT_FOUND");

        supplier.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}