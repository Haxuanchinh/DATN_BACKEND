using SportManager.Domain.Constants;
using SportManager.Domain.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SportManager.Application.CustomerTickets.Models;

public class TicketCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;
}

public class TicketCategoryResponse : BaseModelResopnse
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(500)]
    public string Description { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
}