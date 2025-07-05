using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Entity;

namespace SportManager.Application.ProductReviews.Commands;

public class CreateProductReviewCommentCommand : IRequest<Guid>
{
    public Guid ReviewId { get; set; } // Bắt buộc phải có
    public string Comment { get; set; }
    public Guid? ParentCommentId { get; set; }
}

public class CreateProductReviewCommentHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser)
    : IRequestHandler<CreateProductReviewCommentCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductReviewCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUser.UserId);

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new ApplicationException("Tài khoản chưa được đăng ký.");

        var reviewExists = await _dbContext.ProductReviews
            .AnyAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (!reviewExists)
            throw new ApplicationException("Không tìm thấy đánh giá.");

        if (request.ParentCommentId != null)
        {
            var parentCommentExists = await _dbContext.ProductReviewComments
                .AnyAsync(c => c.Id == request.ParentCommentId, cancellationToken);

            if (!parentCommentExists)
                throw new ApplicationException("Comment cha không tồn tại.");
        }

        var comment = new ProductReviewComment
        {
            Id = Guid.NewGuid(),
            ReviewId = request.ReviewId,
            CustomerId = customer.Id,
            ParentCommentId = request.ParentCommentId,
            Content = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.ProductReviewComments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
