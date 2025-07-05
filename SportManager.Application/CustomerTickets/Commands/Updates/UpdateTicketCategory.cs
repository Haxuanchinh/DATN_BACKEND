using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.CustomerTickets.Commands.Updates;

public class UpdateTicketCategoryCommand : TicketCategoryDto, IRequest<TicketCategoryResponse>
{
    public Guid Id { get; set; }
}

public class UpdateTicketCategoryHandler : IRequestHandler<UpdateTicketCategoryCommand, TicketCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public UpdateTicketCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TicketCategoryResponse> Handle(UpdateTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TicketCategories.FindAsync(request.Id);
        if (entity == null) throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

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
