using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[GenerateDataReader]
public class TaskExtended
{
    public Guid TaskId { get; set; }
    public string UserAgent { get; set; }
    public string Browser { get; set; }
    public string OperatingSystem { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public virtual Task Task { get; set; }
}
