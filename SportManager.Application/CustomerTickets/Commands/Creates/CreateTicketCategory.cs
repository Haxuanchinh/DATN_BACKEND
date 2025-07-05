using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.CustomerTickets.Commands.Creates;

public class CreateTicketCategoryCommand : TicketCategoryDto, IRequest<TicketCategoryResponse>
{
}

public class CreateTicketCategoryHandler : IRequestHandler<CreateTicketCategoryCommand, TicketCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateTicketCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TicketCategoryResponse> Handle(CreateTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new TicketCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.TicketCategories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new TicketCategoryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
