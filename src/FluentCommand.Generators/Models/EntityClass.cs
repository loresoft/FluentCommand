namespace FluentCommand.Generators.Models;

public record EntityClass(
    InitializationMode InitializationMode,
    string FullyQualified,
    string EntityNamespace,
    string EntityName,
    EquatableArray<EntityProperty> Properties
);
