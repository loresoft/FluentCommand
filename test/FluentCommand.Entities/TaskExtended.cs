using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities;

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
    public byte[] RowVersion { get; set; }

    public virtual Task Task { get; set; }
}
