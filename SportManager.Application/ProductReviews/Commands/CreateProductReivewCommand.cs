using SportManager.Application.Abstractions;
using SportManager.Application.Common.Interfaces;
using SportManager.Application.ProductReviews.Models;
using SportManager.Domain.Entity;

namespace SportManager.Application.ProductReviews.Commands;
public class CreateProductReviewCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
}


public class CreateProductReviewHandler(
    IApplicationDbContext _dbContext,
    ICurrentUserService _currentUser)
    : IRequestHandler<CreateProductReviewCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUser.UserId);

        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new ApplicationException("Tài khoản chưa được đăng ký.");

        var review = new ProductReview
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            CustomerId = customer.Id,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.ProductReviews.AddAsync(review, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return review.Id;
    }
}

