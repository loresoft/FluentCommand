using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

public static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor InvalidConstructor { get; } = new DiagnosticDescriptor(
        id: "FCG1001",
        title: "Invalid Constructor",
        messageFormat: "Count not find a constructor with {0} parameter(s) for type {1}.  Classes initialized via constructor need to have the same number of parameters as public properties.",
        category: "FluentCommandGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidConstructorParameter { get; } = new DiagnosticDescriptor(
        id: "FCG1002",
        title: "Invalid Constructor Parameter",
        messageFormat: "Count not find a constructor parameter {0} for type {1}. Classes initialized via constructor need to have parameters that match the properties in the class",
        category: "FluentCommandGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
