using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentCommand.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DataReaderFactoryAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.NoMatchingConstructor,
            DiagnosticDescriptors.ConstructorParameterNotMatched,
            DiagnosticDescriptors.NoMappableProperties,
            DiagnosticDescriptors.UnsupportedPropertyType,
            DiagnosticDescriptors.InvalidGenerateReaderArgument,
            DiagnosticDescriptors.TableAttributeOnInvalidType,
            DiagnosticDescriptors.UnknownGenerateReaderProperty
        );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationAction(AnalyzeCompilation);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        AnalyzeGenerateReaderAttributes(
            context.ReportDiagnostic,
            context.CancellationToken,
            context.Compilation.Assembly.GetAttributes(),
            context.Compilation.Assembly.Name,
            Location.None);

        AnalyzeGenerateReaderAttributes(
            context.ReportDiagnostic,
            context.CancellationToken,
            context.Compilation.SourceModule.GetAttributes(),
            context.Compilation.SourceModule.Name,
            Location.None);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
            return;

        var attributes = typeSymbol.GetAttributes();

        // Check [Table] attribute path
        var tableAttribute = FindSchemaAttribute(attributes, "TableAttribute");
        if (tableAttribute != null)
        {
            if (typeSymbol.IsStatic || typeSymbol.IsAbstract)
            {
                var modifier = typeSymbol.IsStatic ? "static" : "abstract";

                var location = tableAttribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                    ?? typeSymbol.Locations.FirstOrDefault()
                    ?? Location.None;

                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.TableAttributeOnInvalidType,
                    location,
                    typeSymbol.Name,
                    modifier));
            }
            else
            {
                AnalyzeEntityType(context.ReportDiagnostic, context.CancellationToken, typeSymbol);
            }
        }

        AnalyzeGenerateReaderAttributes(
            context.ReportDiagnostic,
            context.CancellationToken,
            attributes,
            typeSymbol.Name,
            typeSymbol.Locations.FirstOrDefault() ?? Location.None);
    }

    private static void AnalyzeGenerateReaderAttributes(
        Action<Diagnostic> reportDiagnostic,
        CancellationToken cancellationToken,
        ImmutableArray<AttributeData> attributes,
        string ownerName,
        Location fallbackLocation)
    {
        foreach (var attr in attributes)
        {
            if (!IsGenerateReaderAttribute(attr))
                continue;

            if (attr.ConstructorArguments.Length != 1 ||
                GetTypeArgument(attr.ConstructorArguments[0]) is not INamedTypeSymbol targetSymbol)
            {
                var location = attr.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? fallbackLocation;

                var diagnostic = Diagnostic.Create(
                    descriptor: DiagnosticDescriptors.InvalidGenerateReaderArgument,
                    location: location,
                    messageArgs: ownerName);

                reportDiagnostic(diagnostic);

                continue;
            }

            var ignoreProperties = GetNamedStringArray(attr, "IgnoreProperties");
            var jsonProperties = GetNamedStringArray(attr, "JsonProperties");

            AnalyzeEntityType(reportDiagnostic, cancellationToken, targetSymbol, ignoreProperties, jsonProperties, attr);
        }
    }

    private static ITypeSymbol? GetTypeArgument(TypedConstant argument)
    {
        return argument.Kind == TypedConstantKind.Type
            ? argument.Value as ITypeSymbol
            : null;
    }

    private static void AnalyzeEntityType(
        Action<Diagnostic> reportDiagnostic,
        CancellationToken cancellationToken,
        INamedTypeSymbol targetSymbol,
        string[]? ignoreProperties = null,
        string[]? jsonPropertyNames = null,
        AttributeData? generateReaderAttribute = null)
    {
        var typeAttributes = targetSymbol.GetAttributes();
        var classIgnored = GetClassIgnoredProperties(typeAttributes);

        if (ignoreProperties != null)
        {
            foreach (var ignoredProperty in ignoreProperties)
                classIgnored.Add(ignoredProperty);
        }

        var jsonProperties = jsonPropertyNames == null
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(jsonPropertyNames, StringComparer.Ordinal);

        var propertySymbols = GetProperties(targetSymbol);

        ignoreProperties ??= [];
        jsonPropertyNames ??= [];

        if (generateReaderAttribute != null)
        {
            AnalyzeGenerateReaderOptions(
                reportDiagnostic: reportDiagnostic,
                cancellationToken: cancellationToken,
                targetSymbol: targetSymbol,
                propertySymbols: propertySymbols,
                ignoreProperties: ignoreProperties,
                jsonProperties: jsonPropertyNames,
                attribute: generateReaderAttribute);
        }

        var hasParameterlessCtor = targetSymbol.Constructors.Any(c => c.Parameters.Length == 0);

        // Count mappable properties
        var mappableProperties = propertySymbols
            .Where(p => IsMappableProperty(p, classIgnored, jsonProperties))
            .ToList();

        // Report unsupported property types
        foreach (var prop in propertySymbols)
        {
            var propertyAttributes = prop.GetAttributes();

            if (classIgnored.Contains(prop.Name) || HasIgnorePropertyAttribute(propertyAttributes) || IsNotMapped(propertyAttributes))
                continue;

            var jsonColumn = GetJsonColumnAttribute(propertyAttributes);
            if (jsonColumn != null)
                continue;

            if (jsonProperties.Contains(prop.Name))
                continue;

            if (!IsSupportedType(prop.Type))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedPropertyType,
                    prop.Locations.FirstOrDefault() ?? Location.None,
                    prop.Name,
                    targetSymbol.Name,
                    prop.Type.ToDisplayString());

                reportDiagnostic(diagnostic);
            }
        }

        // Report no mappable properties
        if (mappableProperties.Count == 0)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.NoMappableProperties,
                targetSymbol.Locations.FirstOrDefault() ?? Location.None,
                targetSymbol.Name);

            reportDiagnostic(diagnostic);
            return;
        }

        // Constructor mode analysis
        if (!hasParameterlessCtor)
        {
            var mappableCount = propertySymbols
                .Count(p => IsMappableProperty(p, classIgnored, jsonProperties));

            var constructor = targetSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == mappableCount);

            if (constructor == null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.NoMatchingConstructor,
                    targetSymbol.Locations.FirstOrDefault() ?? Location.None,
                    targetSymbol.Name,
                    mappableCount);

                reportDiagnostic(diagnostic);
                return;
            }

            // Check for unmatched constructor parameters
            foreach (var parameter in constructor.Parameters)
            {
                var hasMatch = propertySymbols.Any(p =>
                    string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (!hasMatch)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.ConstructorParameterNotMatched,
                        parameter.Locations.FirstOrDefault() ?? constructor.Locations.FirstOrDefault() ?? Location.None,
                        parameter.Name,
                        targetSymbol.Name);

                    reportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void AnalyzeGenerateReaderOptions(
        Action<Diagnostic> reportDiagnostic,
        CancellationToken cancellationToken,
        INamedTypeSymbol targetSymbol,
        List<IPropertySymbol> propertySymbols,
        string[] ignoreProperties,
        string[] jsonProperties,
        AttributeData attribute)
    {
        var location = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation()
            ?? targetSymbol.Locations.FirstOrDefault()
            ?? Location.None;

        var propertyNames = new HashSet<string>(propertySymbols.Select(static p => p.Name), StringComparer.Ordinal);

        ReportUnknownGenerateReaderProperties(reportDiagnostic, targetSymbol, location, propertyNames, "IgnoreProperties", ignoreProperties);
        ReportUnknownGenerateReaderProperties(reportDiagnostic, targetSymbol, location, propertyNames, "JsonProperties", jsonProperties);
    }

    private static void ReportUnknownGenerateReaderProperties(
        Action<Diagnostic> reportDiagnostic,
        INamedTypeSymbol targetSymbol,
        Location location,
        HashSet<string> propertyNames,
        string optionName,
        string[] configuredNames)
    {
        foreach (var configuredName in configuredNames)
        {
            if (propertyNames.Contains(configuredName))
                continue;

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.UnknownGenerateReaderProperty,
                location,
                optionName,
                configuredName,
                targetSymbol.Name);

            reportDiagnostic(diagnostic);
        }
    }

    #region Attribute helpers (mirrors generator logic)

    private static bool IsGenerateReaderAttribute(AttributeData attr)
    {
        return attr.AttributeClass is
        {
            Name: "GenerateReaderAttribute",
            ContainingNamespace:
            {
                Name: "Attributes",
                ContainingNamespace.Name: "FluentCommand"
            }
        };
    }

    private static AttributeData? FindSchemaAttribute(ImmutableArray<AttributeData> attributes, string name)
    {
        return attributes.FirstOrDefault(a =>
            a.AttributeClass is
            {
                ContainingNamespace:
                {
                    Name: "Schema",
                    ContainingNamespace:
                    {
                        Name: "DataAnnotations",
                        ContainingNamespace:
                        {
                            Name: "ComponentModel",
                            ContainingNamespace.Name: "System"
                        }
                    }
                }
            }
            && a.AttributeClass.Name == name
        );
    }

    private static bool HasIgnorePropertyAttribute(ImmutableArray<AttributeData> attributes)
    {
        return attributes.Any(a => a.AttributeClass is
        {
            Name: "IgnorePropertyAttribute",
            ContainingNamespace:
            {
                Name: "Attributes",
                ContainingNamespace.Name: "FluentCommand"
            }
        });
    }

    private static bool HasJsonColumnAttribute(ImmutableArray<AttributeData> attributes)
    {
        return GetJsonColumnAttribute(attributes) != null;
    }

    private static AttributeData? GetJsonColumnAttribute(ImmutableArray<AttributeData> attributes)
    {
        return attributes.FirstOrDefault(a => a.AttributeClass is
        {
            Name: "JsonColumnAttribute",
            ContainingNamespace:
            {
                Name: "Attributes",
                ContainingNamespace.Name: "FluentCommand"
            }
        });
    }

    private static bool IsNotMapped(ImmutableArray<AttributeData> attributes)
    {
        return FindSchemaAttribute(attributes, "NotMappedAttribute") != null;
    }

    private static bool IsMappableProperty(IPropertySymbol propertySymbol, HashSet<string> classIgnored, HashSet<string>? jsonProperties = null)
    {
        var attributes = propertySymbol.GetAttributes();
        if (classIgnored.Contains(propertySymbol.Name) || HasIgnorePropertyAttribute(attributes) || IsNotMapped(attributes))
            return false;

        return jsonProperties?.Contains(propertySymbol.Name) == true || HasJsonColumnAttribute(attributes) || IsSupportedType(propertySymbol.Type);
    }

    private static string[] GetNamedStringArray(AttributeData attribute, string argName)
    {
        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key != argName || namedArg.Value.Kind != TypedConstantKind.Array)
                continue;

            return namedArg.Value.Values
                .Select(static v => v.Value)
                .OfType<string>()
                .ToArray();
        }

        return [];
    }

    private static HashSet<string> GetClassIgnoredProperties(ImmutableArray<AttributeData> attributes)
    {
        var ignored = new HashSet<string>(StringComparer.Ordinal);

        foreach (var attr in attributes)
        {
            if (attr.AttributeClass is not
                {
                    Name: "IgnorePropertyAttribute",
                    ContainingNamespace:
                    {
                        Name: "Attributes",
                        ContainingNamespace.Name: "FluentCommand"
                    }
                })
            {
                continue;
            }

            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string ctorName)
            {
                ignored.Add(ctorName);
                continue;
            }

            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "PropertyName" && namedArg.Value.Value is string namedValue)
                    ignored.Add(namedValue);
            }
        }

        return ignored;
    }

    private static List<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>();
        var currentSymbol = targetSymbol;

        while (currentSymbol != null)
        {
            var propertySymbols = currentSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsIndexer
                    && !p.IsAbstract
                    && p.DeclaredAccessibility == Accessibility.Public
                    && !properties.ContainsKey(p.Name)
                );

            foreach (var propertySymbol in propertySymbols)
                properties.Add(propertySymbol.Name, propertySymbol);

            currentSymbol = currentSymbol.BaseType;
        }

        return properties.Values.ToList();
    }

    private static bool IsSupportedType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
            return IsSupportedType(namedType.TypeArguments[0]);

        if (type.TypeKind == TypeKind.Enum)
            return true;

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_Char:
            case SpecialType.System_Decimal:
            case SpecialType.System_Double:
            case SpecialType.System_Single:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_String:
                return true;
        }

        if (type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte })
            return true;

        var fullName = type.ToDisplayString();

        return fullName is
            "System.DateTime" or
            "System.DateTimeOffset" or
            "System.Guid" or
            "System.TimeSpan" or
            "System.DateOnly" or
            "System.TimeOnly" or
            "FluentCommand.ConcurrencyToken";
    }

    #endregion
}
