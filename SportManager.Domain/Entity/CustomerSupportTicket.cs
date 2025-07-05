using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;
[Table(TableNameConstants.CustomerSuportTicket)]
public class CustomerSuportTicket : EntityBase<Guid>
{
    [ForeignKey("Customer")]
    public Guid? CustomerId { get; set; }

    public virtual Customer? Customer { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; }

    [Required]
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    [Required]
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    public DateTime? ClosedAt { get; set; }

    [Required]
    [ForeignKey("Category")]
    public Guid CategoryId { get; set; }

    public virtual TicketCategory Category { get; set; }

    [InverseProperty("Ticket")]
    public virtual ICollection<TicketReply> Replies { get; set; } = new HashSet<TicketReply>();

    [InverseProperty("SuportTicket")]
    public virtual ICollection<CustomerStatisfactionRating> Ratings { get; set; } = new HashSet<CustomerStatisfactionRating>();
}
public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Normal,
    High,
    Urgent
}