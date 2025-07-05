using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Category.Commands.Delete;

public record DeleteCategoryCommand(Guid Id) : IRequest<int>;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(ErrorCode.FIELD_REQUIRED);
    }
}

public class DeleteCategoryCommandHandler(IApplicationDbContext applicationDbContext)
    : IRequestHandler<DeleteCategoryCommand, int>
{
    public async Task<int> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var customer = await applicationDbContext.Categories
            .Where(p => p.Id.Equals(request.Id))
            .FirstOrDefaultAsync(cancellationToken);

        if (customer == null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, ErrorCode.NOT_FOUND);
        }

        applicationDbContext.Categories.Remove(customer);
        var result = await applicationDbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
