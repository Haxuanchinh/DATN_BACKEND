using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.ProductReview)]
public class ProductReview : EntityBase<Guid>
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public Guid? VariantId { get; set; } // optional
    public ProductVariant? Variant { get; set; }
    public string[]? Images { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }

    public int Rating { get; set; }
    public string Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }

    public ICollection<ProductReviewComment> Comments { get; set; } // Cho mục 2 bên dưới
}


[Table(TableNameConstants.ProductReviewComment)]
public class ProductReviewComment : EntityBase<Guid>
{
    public Guid ReviewId { get; set; }
    public ProductReview Review { get; set; }

    public Guid? ParentCommentId { get; set; } // Để support reply nhiều cấp
    public ProductReviewComment? ParentComment { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }

    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductReviewComment>? Replies { get; set; }
}
