using SportManager.Domain.Entity;

namespace SportManager.Application.ProductReviews.Models;

public class ProductReviewDto
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public Guid? ParentCommentId { get; set; }
    // public bool IsVerifiedPurchase { get; set; }
}
