using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[Table("User")]
public class UserImport
{
    public string EmailAddress { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLogin { get; set; }

}
