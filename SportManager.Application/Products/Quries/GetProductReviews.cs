using SportManager.Application.Common.Interfaces;
using SportManager.Application.Products.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.Products.Quries;

public record GetProductReviewsQuery(Guid ProductId) : IRequest<List<ProductReviewDto>>;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, List<ProductReviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Lấy reviews + comments + replies + thông tin customer
            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == request.ProductId)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.Customer)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (reviews == null || reviews.Count == 0)
                return [];

            // Lấy toàn bộ CustomerIds của reviews và comments
            var customerIds = reviews
                .Select(r => r.CustomerId)
                .Union(reviews.SelectMany(r => r.Comments)
                              .SelectMany(c => c.Replies ?? [])
                              .Select(c => c.CustomerId))
                .Union(reviews.SelectMany(r => r.Comments).Select(c => c.CustomerId))
                .Distinct()
                .ToList();

            var userMap = await _context.Users
                .Where(u => u.CustomerProfile != null && customerIds.Contains(u.CustomerProfile.Id))
                .Select(u => new
                {
                    CustomerId = u.CustomerProfile.Id,
                    Username = u.Username
                })
                .ToDictionaryAsync(x => x.CustomerId, x => x.Username, cancellationToken);

            // Map reviews
            var reviewDtos = reviews.Select(r => new ProductReviewDto(
                Id: r.Id,
                ProductId: r.ProductId,
                VariantId: r.VariantId,
                CustomerId: r.CustomerId,
                UserName: userMap.GetValueOrDefault(r.CustomerId) ?? "Ẩn danh",
                Rating: r.Rating,
                Comment: r.Comment,
                IsVerifiedPurchase: r.IsVerifiedPurchase,
                Images: r.Images?.ToList(),
                Comments: MapComments(r.Comments, userMap)
            )).ToList();

            return reviewDtos;
        }
        catch (Exception ex)
        {
            // Ghi log hoặc xử lý nếu cần
            // _logger.LogError(ex, "Error getting product reviews.");
            throw new ApplicationException("Đã xảy ra lỗi khi lấy đánh giá sản phẩm.", "Lỗi");
        }
    }

    private static List<ProductReviewCommentDto> MapComments(
        ICollection<ProductReviewComment> comments,
        Dictionary<Guid, string> userMap)
    {
        if (comments == null || !comments.Any()) return [];

        var commentDict = comments.ToDictionary(c => c.Id, c => new ProductReviewCommentDto(
            Id: c.Id,
            ParentCommentId: c.ParentCommentId,
            CustomerId: c.CustomerId,
            UserName: userMap.GetValueOrDefault(c.CustomerId) ?? "Ẩn danh",
            Content: c.Content,
            CreatedAt: c.CreatedAt,
            Replies: new List<ProductReviewCommentDto>()
        ));

        var rootComments = new List<ProductReviewCommentDto>();

        foreach (var comment in commentDict.Values)
        {
            if (comment.ParentCommentId.HasValue && commentDict.TryGetValue(comment.ParentCommentId.Value, out var parent))
            {
                parent.Replies.Add(comment);
            }
            else
            {
                rootComments.Add(comment);
            }
        }

        return rootComments.OrderByDescending(c => c.CreatedAt).ToList();
    }
}
