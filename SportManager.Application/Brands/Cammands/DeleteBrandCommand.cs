using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Brands.Cammands;

public class DeleteBrandCommand : IRequest<Unit>
{
    public Guid Id { get; set; }

    public DeleteBrandCommand(Guid id)
    {
        Id = id;
    }
}

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteBrandCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var brand = await _dbContext.Brands
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (brand is null)
            throw new ApplicationException(ErrorCode.NOT_FOUND, "brand");

        _dbContext.Brands.Remove(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
