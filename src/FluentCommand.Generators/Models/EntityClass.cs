namespace FluentCommand.Generators.Models;

public record EntityClass
{
    public InitializationMode InitializationMode { get; init; }
    public string FullyQualified { get; init; } = null!;
    public string EntityNamespace { get; init; } = null!;
    public string EntityName { get; init; } = null!;
    public EquatableArray<EntityProperty> Properties { get; init; }
    public string? TableName { get; init; }
    public string? TableSchema { get; init; }
}
