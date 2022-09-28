using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities;

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
    public byte[] RowVersion { get; set; }

    public virtual Task Task { get; set; }
    public virtual User User { get; set; }
}
