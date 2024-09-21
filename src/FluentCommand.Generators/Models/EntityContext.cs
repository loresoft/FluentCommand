using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators.Models;

public record EntityContext(
    EquatableArray<EntityClass> EntityClasses,
    EquatableArray<Diagnostic> Diagnostics
);
