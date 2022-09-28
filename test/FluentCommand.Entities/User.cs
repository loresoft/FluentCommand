using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

public class User
{
    public Guid Id { get; set; }
    public string EmailAddress { get; set; }
    public bool IsEmailAddressConfirmed { get; set; }
    public string DisplayName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public string ResetHash { get; set; }
    public string InviteHash { get; set; }
    public int AccessFailedCount { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[] RowVersion { get; set; }


    [NotMapped]
    public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();

    [NotMapped]
    public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();

    [NotMapped]
    public virtual ICollection<Task> CreatedTasks { get; set; } = new List<Task>();

    [NotMapped]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

}
