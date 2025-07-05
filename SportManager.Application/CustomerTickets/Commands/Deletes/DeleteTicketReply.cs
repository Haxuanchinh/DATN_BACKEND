using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;

namespace SportManager.Application.CustomerTickets.Commands.Deletes;

public class DeleteCustomerSupportReplyCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteCustomerSupportReplyHandler : IRequestHandler<DeleteCustomerSupportReplyCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomerSupportReplyHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCustomerSupportReplyCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TicketReplies
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new ApplicationException(ErrorCode.NOT_FOUND, $"Reply {request.Id} not found");

        _context.TicketReplies.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
