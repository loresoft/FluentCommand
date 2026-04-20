using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

/// <summary>
/// Centralized diagnostic descriptors for the DataReaderFactory source generator and analyzer.
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Usage";

    /// <summary>
    /// FLC001: Type has no parameterless constructor and no constructor matching mappable property count.
    /// </summary>
    public static readonly DiagnosticDescriptor NoMatchingConstructor = new(
        id: "FLC001",
        title: "No matching constructor found",
        messageFormat: "Type '{0}' has no parameterless constructor and no constructor with {1} parameters matching the mappable property count",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator requires either a parameterless constructor (for object initializer mode) or a constructor whose parameter count matches the number of mappable properties."
    );

    /// <summary>
    /// FLC002: Constructor parameter does not match any property.
    /// </summary>
    public static readonly DiagnosticDescriptor ConstructorParameterNotMatched = new(
        id: "FLC002",
        title: "Constructor parameter has no matching property",
        messageFormat: "Constructor parameter '{0}' on type '{1}' does not match any public property (case-insensitive)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When using constructor initialization, every constructor parameter must match a public property name (case-insensitive)."
    );

    /// <summary>
    /// FLC003: Type has no mappable properties.
    /// </summary>
    public static readonly DiagnosticDescriptor NoMappableProperties = new(
        id: "FLC003",
        title: "No mappable properties found",
        messageFormat: "Type '{0}' has no mappable properties; the generated reader will not map any columns",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All public properties are either ignored, not-mapped, or have unsupported types. The generated data reader extension will produce an empty mapping."
    );

    /// <summary>
    /// FLC004: Property type is not supported for data reader mapping.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedPropertyType = new(
        id: "FLC004",
        title: "Unsupported property type",
        messageFormat: "Property '{0}' on type '{1}' has unsupported type '{2}' and will not be mapped",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The property type is not a supported primitive, enum, or known struct type. The property will be excluded from data reader mapping."
    );

    /// <summary>
    /// FLC005: [GenerateReader] attribute has an invalid type argument.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidGenerateReaderArgument = new(
        id: "FLC005",
        title: "Invalid GenerateReader type argument",
        messageFormat: "The [GenerateReader] attribute on '{0}' has an invalid or missing type argument",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The [GenerateReader(typeof(T))] attribute requires exactly one type argument that resolves to a named type."
    );

    /// <summary>
    /// FLC006: [Table] attribute applied to a static or abstract type.
    /// </summary>
    public static readonly DiagnosticDescriptor TableAttributeOnInvalidType = new(
        id: "FLC006",
        title: "Table attribute on static or abstract type",
        messageFormat: "The [Table] attribute on '{0}' is ignored because the type is {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator skips static and abstract types annotated with [Table]. Remove the attribute or make the type non-static and non-abstract."
    );
}
