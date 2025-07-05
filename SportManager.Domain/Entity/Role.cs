using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Role)]
public class Role : EntityBase<Guid>
{
    public string Name { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}
