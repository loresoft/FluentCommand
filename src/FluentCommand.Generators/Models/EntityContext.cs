using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators.Models;

public record EntityContext(
    EntityClass EntityClass,
    EquatableArray<Diagnostic> Diagnostics
);
