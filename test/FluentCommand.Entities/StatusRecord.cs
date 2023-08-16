using System;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Handlers;

namespace FluentCommand.Entities;

[Table(nameof(StatusRecord))]
public record StatusRecord(
    int Id,
    string Name,
    string Description,
    int DisplayOrder,
    bool IsActive,
    DateTimeOffset Created,
    string CreatedBy,
    DateTimeOffset Updated,
    string UpdatedBy,
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    ConcurrencyToken RowVersion
);
