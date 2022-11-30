using System;

namespace FluentCommand.Entities;

[GenerateDataReader]
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
    ConcurrencyToken RowVersion
);
