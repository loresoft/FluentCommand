using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Handlers;

namespace FluentCommand.Entities;

[Table(nameof(StatusReadOnly))]
public class StatusReadOnly
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset Created { get; init; }
    public string CreatedBy { get; init; }
    public DateTimeOffset Updated { get; init; }
    public string UpdatedBy { get; init; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; init; }
}
