using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[Table(nameof(UserRole))]
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    [NotMapped]
    public virtual User User { get; set; }
    [NotMapped]
    public virtual Role Role { get; set; }
}
