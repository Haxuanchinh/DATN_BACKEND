using SportManager.Application.Abstractions;
using SportManager.Application.Common.Exception;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;

namespace SportManager.Application.CustomerTickets.Queries.GetById;

public class GetTicketByIdCustomerQuery : IRequest<TicketCategoryResponse>
{
}

public class GetTicketByIdCustomerHandler : IRequestHandler<GetTicketByIdCustomerQuery, TicketCategoryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTicketByIdCustomerHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUserService = currentUser;
    }

    public async Task<TicketCategoryResponse> Handle(GetTicketByIdCustomerQuery _, CancellationToken cancellationToken)
    {
        var customerId = _currentUserService.CustomerId;
        if (customerId == null)
        {
            throw new UnauthorizedAccessException("User is not a customer");
        }

        var entity = await _context.CustomerSuportTickets.Where(x => x.CustomerId == Guid.Parse(customerId)).FirstOrDefaultAsync(cancellationToken);
        if (entity == null) throw new ApplicationException(ErrorCode.NOT_FOUND, customerId);

        return new TicketCategoryResponse
        {
            //Id = entity.Id,
            //Name = entity.Name,
            //Description = entity.Description,
            //IsActive = entity.IsActive,
            //CreatedAt = entity.CreatedAt,
        };
    }
}