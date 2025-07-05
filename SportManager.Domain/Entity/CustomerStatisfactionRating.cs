using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.CustomerSatisfactionRating)]
public class CustomerStatisfactionRating : EntityBase<Guid>
{
    [Required]
    [ForeignKey("SuportTicket")]
    public Guid TicketId { get; set; }

    [Required]
    public virtual CustomerSuportTicket SuportTicket { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string Comment { get; set; }

    [Required]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
