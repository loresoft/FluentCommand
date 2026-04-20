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
            DiagnosticDescriptors.TableAttributeOnInvalidType
        );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
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
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.TableAttributeOnInvalidType,
                    tableAttribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                        ?? typeSymbol.Locations.FirstOrDefault() ?? Location.None,
                    typeSymbol.Name,
                    modifier));
            }
            else
            {
                AnalyzeEntityType(context, typeSymbol);
            }
        }

        // Check [GenerateReader] attribute path
        foreach (var attr in attributes)
        {
            if (!IsGenerateReaderAttribute(attr))
                continue;

            if (attr.ConstructorArguments.Length != 1 ||
                attr.ConstructorArguments[0].Value is not INamedTypeSymbol targetSymbol)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidGenerateReaderArgument,
                    attr.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken).GetLocation()
                        ?? typeSymbol.Locations.FirstOrDefault() ?? Location.None,
                    typeSymbol.Name));
                continue;
            }

            AnalyzeEntityType(context, targetSymbol);
        }
    }

    private static void AnalyzeEntityType(SymbolAnalysisContext context, INamedTypeSymbol targetSymbol)
    {
        var typeAttributes = targetSymbol.GetAttributes();
        var classIgnored = GetClassIgnoredProperties(typeAttributes);
        var propertySymbols = GetProperties(targetSymbol);

        var hasParameterlessCtor = targetSymbol.Constructors.Any(c => c.Parameters.Length == 0);

        // Count mappable properties
        var mappableProperties = propertySymbols
            .Where(p => !classIgnored.Contains(p.Name)
                        && !HasIgnorePropertyAttribute(p.GetAttributes())
                        && IsSupportedType(p.Type))
            .ToList();

        // Report unsupported property types
        foreach (var prop in propertySymbols)
        {
            if (classIgnored.Contains(prop.Name) || HasIgnorePropertyAttribute(prop.GetAttributes()))
                continue;

            if (!IsSupportedType(prop.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedPropertyType,
                    prop.Locations.FirstOrDefault() ?? Location.None,
                    prop.Name,
                    targetSymbol.Name,
                    prop.Type.ToDisplayString()));
            }
        }

        // Report no mappable properties
        if (mappableProperties.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NoMappableProperties,
                targetSymbol.Locations.FirstOrDefault() ?? Location.None,
                targetSymbol.Name));
            return;
        }

        // Constructor mode analysis
        if (!hasParameterlessCtor)
        {
            var mappableCount = propertySymbols
                .Count(p => !classIgnored.Contains(p.Name) && !HasIgnorePropertyAttribute(p.GetAttributes()));

            var constructor = targetSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == mappableCount);

            if (constructor == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NoMatchingConstructor,
                    targetSymbol.Locations.FirstOrDefault() ?? Location.None,
                    targetSymbol.Name,
                    mappableCount));
                return;
            }

            // Check for unmatched constructor parameters
            foreach (var parameter in constructor.Parameters)
            {
                var hasMatch = propertySymbols.Any(p =>
                    string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (!hasMatch)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ConstructorParameterNotMatched,
                        parameter.Locations.FirstOrDefault() ?? constructor.Locations.FirstOrDefault() ?? Location.None,
                        parameter.Name,
                        targetSymbol.Name));
                }
            }
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
                .Where(p => !p.IsIndexer && !p.IsAbstract && p.DeclaredAccessibility == Accessibility.Public)
                .Where(p => !properties.ContainsKey(p.Name));

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
