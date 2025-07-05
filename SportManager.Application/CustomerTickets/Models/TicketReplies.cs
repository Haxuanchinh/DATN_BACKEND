using System.ComponentModel.DataAnnotations;

namespace SportManager.Application.CustomerTickets.Models;

public class CustomerSupportReplyDto
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int? StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; }

    public DateTime? RepliedAt { get; set; } = DateTime.UtcNow;

    public string[]? Images { get; set; }
}

public class CustomerSupportReplyResponse : BaseModelResopnse
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int? StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string Message { get; set; }
    public DateTime RepliedAt { get; set; }
    public string[]? Images { get; set; }
    public int ChildCount { get; set; }

}
