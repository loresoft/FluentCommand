using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Handlers;

namespace FluentCommand.Entities;

[Table(nameof(Task))]
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

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();
    [NotMapped]
    public virtual Priority Priority { get; set; }
    [NotMapped]
    public virtual Status Status { get; set; }
    [NotMapped]
    public virtual User AssignedUser { get; set; }
    [NotMapped]
    public virtual User CreatedUser { get; set; }
    [NotMapped]
    public virtual TaskExtended TaskExtended { get; set; }
}
