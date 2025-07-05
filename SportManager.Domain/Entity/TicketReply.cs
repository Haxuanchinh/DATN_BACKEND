using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.TicketReply)]
public class TicketReply : EntityBase<Guid>
{
    [Required]
    [ForeignKey("Ticket")]
    public Guid TicketId { get; set; }

    [Required]
    public virtual CustomerSuportTicket Ticket { get; set; }
    public Guid? ParentId { get; set; }
    public virtual TicketReply? Parent { get; set; }
    public virtual ICollection<TicketReply> Children { get; set; } = new List<TicketReply>();

    [ForeignKey("User")]
    public Guid? UserId { get; set; }

    public virtual User? User { get; set; }

    [ForeignKey("Staff")]
    public int? StaffId { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime RepliedAt { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "text")]
    public string? ImagesJson { get; set; }

    [NotMapped]
    public string[]? Images
    {
        get => ImagesJson != null ? System.Text.Json.JsonSerializer.Deserialize<string[]>(ImagesJson) : null;
        set => ImagesJson = value != null ? System.Text.Json.JsonSerializer.Serialize(value) : null;
    }
}
