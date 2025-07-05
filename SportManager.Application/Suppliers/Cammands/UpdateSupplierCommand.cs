using SportManager.Application.Suppliers.Models;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Supplier.Commands.Update
{
    public class UpdateSupplierCommand : SuplierDto, IRequest<Unit>
    {
        public Guid Id { get; set; }

        public void Normalize()
        {
            Name = Name?.Trim();
        }
    }

    public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierCommand>
    {
        public UpdateSupplierValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id là bắt buộc");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên nhà cung cấp là bắt buộc");
            RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.Phone).NotEmpty();
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

            // Kiểm tra trùng tên (trừ chính nó)
            var duplicateName = await _dbContext.Suppliers
                .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.Id != request.Id, cancellationToken);

            if (duplicateName)
            {
                throw new ApplicationException("DUPLICATE_NAME");
            }

            var supplier = await _dbContext.Suppliers.FindAsync(new object[] { request.Id }, cancellationToken);

            if (supplier == null)
                throw new KeyNotFoundException("SUPPLIER_NOT_FOUND");

            // Cập nhật thông tin
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
}