using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.ShipperProfile)]
public class ShipperProfile : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public virtual User User { get; set; }
}
