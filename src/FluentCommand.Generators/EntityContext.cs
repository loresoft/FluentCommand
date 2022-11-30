using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

public record EntityContext(
    EntityClass EntityClass,
    ImmutableArray<Diagnostic> Diagnostics
);

public record EntityClass(
    InitializationMode InitializationMode,
    string EntityNamespace,
    string EntityName,
    ImmutableArray<EntityProperty> Properties
);

public record EntityProperty(
    string PropertyName,
    string PropertyType,
    string ParameterName = null
);

public enum InitializationMode
{
    ObjectInitializer,
    Constructor
}
