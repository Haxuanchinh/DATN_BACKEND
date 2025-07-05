using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Products.Commands.Delete;

public record DeleteProductCommand(Guid Id) : IRequest<int>;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class DeleteProductCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<DeleteProductCommand, int>
{
    public async Task<int> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var customer = await applicationDbContext.Products
            .Where(p => p.Id.Equals(request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        applicationDbContext.Products.Remove(customer);
        var result = await applicationDbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
