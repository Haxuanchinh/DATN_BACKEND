using SportManager.Application.Categroies.Models;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.Categories.Commands.Create;

// Update the command to accept multiple categories
public class CreateCategoryCommand : IRequest<CreateCategoryResponse>
{
    public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}

// Update the response to return multiple IDs
public class CreateCategoryResponse
{
    public List<Guid> Ids { get; set; } = new List<Guid>();
    public List<string> Errors { get; set; } = new List<string>();
}
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCategoryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = new CreateCategoryResponse();

        // Check for duplicates first
        var existingNames = await _dbContext.Categories
            .Select(c => c.Name.ToLower())
            .ToListAsync(cancellationToken);

        var duplicateNames = request.Categories
            .Select(c => c.Name.ToLower())
            .Intersect(existingNames)
            .ToList();

        if (duplicateNames.Any())
        {
            throw new ApplicationException($"Duplicate category names: {string.Join(", ", duplicateNames)}");
        }

        // All names are unique, proceed with creation
        var categoriesToAdd = request.Categories.Select(categoryDto => new Domain.Entity.Category
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            Logo = categoryDto.Logo,
        }).ToList();

        await _dbContext.Categories.AddRangeAsync(categoriesToAdd, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        response.Ids.AddRange(categoriesToAdd.Select(c => c.Id));
        return response;
    }
}


