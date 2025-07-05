using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;

namespace SportManager.Application.CustomerTickets.Queries.GetPagings;

public class GetRepliesQuery : BaseModelRequest, IRequest<List<CustomerSupportReplyResponse>>
{
    public Guid TicketId { get; set; }
    public Guid? ParentId { get; set; }
}

public class GetRepliesHandler : IRequestHandler<GetRepliesQuery, List<CustomerSupportReplyResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetRepliesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerSupportReplyResponse>> Handle(GetRepliesQuery request, CancellationToken cancellationToken)
    {
        var replies = await _context.TicketReplies
            .Where(r => r.TicketId == request.TicketId && r.ParentId == request.ParentId)
            .OrderBy(r => r.RepliedAt)
            .Skip((request.PageNumber) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new CustomerSupportReplyResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                StaffId = r.StaffId,
                Message = r.Message,
                RepliedAt = r.RepliedAt,
                Images = r.Images,
                UserName = r.User != null ? r.User.Username : "",
                StaffName = r.StaffId != null ? "Nhân viên #" + r.StaffId : "",
                ChildCount = _context.TicketReplies.Count(x => x.ParentId == r.Id)
            })
            .ToListAsync(cancellationToken);

        return replies;
    }
}
