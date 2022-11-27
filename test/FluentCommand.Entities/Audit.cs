using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FluentCommand.Entities;

[GenerateDataReader]
public class Audit
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? UserId { get; set; }
    public int? TaskId { get; set; }
    public string Content { get; set; }
    public string Username { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public virtual Task Task { get; set; }
    [NotMapped]
    public virtual User User { get; set; }
}
