using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;

namespace SportManager.Application.CustomerTickets.Queries.GetById;

public class GetTicketCategoryByIdQuery : IRequest<TicketCategoryResponse>
{
    public Guid Id { get; set; }
}

public class GetTicketCategoryByIdHandler : IRequestHandler<GetTicketCategoryByIdQuery, TicketCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public GetTicketCategoryByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TicketCategoryResponse> Handle(GetTicketCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.TicketCategories.FindAsync(request.Id, cancellationToken);
        if (entity == null) throw new ApplicationException(ErrorCode.NOT_FOUND, request.Id.ToString());

        return new TicketCategoryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
        };
    }
}