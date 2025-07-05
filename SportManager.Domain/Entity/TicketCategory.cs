using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.TicketCategory)]
public class TicketCategory : EntityBase<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [InverseProperty("Category")]
    public virtual ICollection<CustomerSuportTicket> Tickets { get; set; } = new HashSet<CustomerSuportTicket>();
}
