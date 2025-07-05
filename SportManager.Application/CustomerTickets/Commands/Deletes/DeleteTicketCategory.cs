using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.CustomerTickets.Commands.Deletes;

public class DeleteTicketCategoryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteTicketCategoryHandler : IRequestHandler<DeleteTicketCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteTicketCategoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTicketCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TicketCategories.FindAsync(request.Id, cancellationToken);
        if (entity == null) throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());

        _context.TicketCategories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
