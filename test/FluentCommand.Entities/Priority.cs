using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[GenerateDataReader]
public class Priority
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
