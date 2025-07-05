using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Carts.Commands.Delete;

public record DeleteCartCommand(Guid Id) : IRequest<int>;

public class DeleteCartCommandValidator : AbstractValidator<DeleteCartCommand>
{
    public DeleteCartCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class DeleteCartCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<DeleteCartCommand, int>
{
    public async Task<int> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        var customer = await applicationDbContext.CartItems
            .Where(p => p.Id.Equals(request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        applicationDbContext.CartItems.Remove(customer);
        var result = await applicationDbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
