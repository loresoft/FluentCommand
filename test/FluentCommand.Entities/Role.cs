using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Handlers;

namespace FluentCommand.Entities;

[GenerateDataReader]
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
