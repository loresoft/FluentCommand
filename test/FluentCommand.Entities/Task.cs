using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities;

public class Task
{
    public Guid Id { get; set; }
    public int StatusId { get; set; }
    public int? PriorityId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? CompleteDate { get; set; }
    public Guid? AssignedId { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }
    public byte[] RowVersion { get; set; }

    public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();
    public virtual Priority Priority { get; set; }
    public virtual Status Status { get; set; }
    public virtual User AssignedUser { get; set; }
    public virtual User CreatedUser { get; set; }
    public virtual TaskExtended TaskExtended { get; set; }
}
