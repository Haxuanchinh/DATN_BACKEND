using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Customer.Commands.Delete;

public record DeleteCustomerCommand(Guid Id) : IRequest<int>;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class DeleteCustomerCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<DeleteCustomerCommand, int>
{
    public async Task<int> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await applicationDbContext.Customers
            .Where(p => p.Id.Equals(request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        applicationDbContext.Customers.Remove(customer);
        var result = await applicationDbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
