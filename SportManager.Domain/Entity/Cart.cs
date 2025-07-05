using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Cart)]
public class Cart : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public ICollection<CartItem> Items { get; set; }
}
