using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Handlers;

namespace FluentCommand.Entities;

[Table(nameof(StatusConstructor))]
public class StatusConstructor
{
    public StatusConstructor(
        int id,
        string name,
        string description,
        bool isActive,
        int displayOrder,
        DateTimeOffset created,
        string createdBy,
        DateTimeOffset updated,
        string updatedBy,
        ConcurrencyToken rowVersion)
    {
        Id = id;
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        Created = created;
        CreatedBy = createdBy;
        Updated = updated;
        UpdatedBy = updatedBy;
        RowVersion = rowVersion;
    }

    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public int DisplayOrder { get; }
    public bool IsActive { get; }
    public DateTimeOffset Created { get; }
    public string CreatedBy { get; }
    public DateTimeOffset Updated { get; }
    public string UpdatedBy { get; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; }
}
