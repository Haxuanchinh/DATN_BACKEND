using SportManager.Application.Categroies.Models;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Categories.Queries;


public class GetQueryByIdQueryResponse : CategoryDto
{
    public Guid Id { get; set; }
}
public class GetCategoryByIdQuery : IRequest<GetQueryByIdQueryResponse>
{
    public Guid Id { get; set; }

    public GetCategoryByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, GetQueryByIdQueryResponse>
{
    private readonly IReadOnlyApplicationDbContext _dbContext;

    public GetCategoryByIdQueryHandler(IReadOnlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetQueryByIdQueryResponse> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entity = await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new GetQueryByIdQueryResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Logo = x.Logo
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            throw new ApplicationException(ErrorCode.NOT_FOUND, "Category Not found");
        }

        return entity;
    }
}