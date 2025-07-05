using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;
using SportManager.Domain.Entity;
using System.Text.Json;

namespace SportManager.Application.CustomerTickets.Commands.Creates;

public class CreateCustomerSupportReplyCommand : CustomerSupportReplyDto, IRequest<Guid>
{
    public Guid TicketId { get; set; }
}

public class CreateCustomerSupportReplyHandler : IRequestHandler<CreateCustomerSupportReplyCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerSupportReplyHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCustomerSupportReplyCommand request, CancellationToken cancellationToken)
    {
        var entity = new TicketReply
        {
            TicketId = request.TicketId,
            UserId = request.UserId,
            StaffId = request.StaffId,
            Message = request.Message,
            RepliedAt = DateTime.UtcNow,
            ImagesJson = request.Images != null ? JsonSerializer.Serialize(request.Images) : null
        };

        _context.TicketReplies.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

