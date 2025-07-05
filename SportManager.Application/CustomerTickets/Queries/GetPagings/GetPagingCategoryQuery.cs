using SportManager.Application.Common.Interfaces;
using SportManager.Application.CustomerTickets.Models;
using SportManager.Application.Utilities;

namespace SportManager.Application.CustomerTickets.Queries.GetPagings;

public class GetsPagingTicketCategoriesQuery : BaseModelRequest, IRequest<PageResult<TicketCategoryResponse>> { }

public class GetsPagingQueryHandler(IReadOnlyApplicationDbContext dbContext)
    : IRequestHandler<GetsPagingTicketCategoriesQuery, PageResult<TicketCategoryResponse>>
{
    public async Task<PageResult<TicketCategoryResponse>> Handle(GetsPagingTicketCategoriesQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var keyword = request.Keyword?.Trim();
        // Using AsNoTracking for better performance in read-only queries
        var query = dbContext.TicketCategories
            .WhereIf(!string.IsNullOrEmpty(keyword), ticketCategory =>
                EF.Functions.ILike(ticketCategory.Name, $"%{keyword!}%"))
            .Select(ticketCategory => new TicketCategoryResponse
            {
                Id = ticketCategory.Id,
                Description = ticketCategory.Description,
                IsActive = ticketCategory.IsActive,
                Name = ticketCategory.Name,
                CreatedAt = ticketCategory.CreatedAt,
            })
            .OrderByDescending(ticketCategory => ticketCategory.CreatedAt);
        return await PageResult<TicketCategoryResponse>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}