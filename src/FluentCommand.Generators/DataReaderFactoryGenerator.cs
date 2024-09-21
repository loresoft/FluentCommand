using System.Collections.Immutable;

using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

public abstract class DataReaderFactoryGenerator
{
    protected static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    protected static void WriteSource(SourceProductionContext context, EntityClass entityClass)
    {
        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = DataReaderFactoryWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}DataReaderExtensions.g.cs", source);
    }

    protected static EntityClass CreateClass(Location location, INamedTypeSymbol targetSymbol, List<Diagnostic> diagnostics)
    {
        if (targetSymbol == null)
            return null;

        var fullyQualified = targetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        var mode = targetSymbol.Constructors.Any(c => c.Parameters.Length == 0)
            ? InitializationMode.ObjectInitializer
            : InitializationMode.Constructor;

        var propertySymbols = GetProperties(targetSymbol);

        if (mode == InitializationMode.ObjectInitializer)
        {
            var propertyArray = propertySymbols
                .Select(p => CreateProperty(p))
                .ToArray();

            var entity = new EntityClass(mode, fullyQualified, classNamespace, className, propertyArray);
            return entity;
        }

        // constructor initialization

        // constructor with same number of parameters as properties
        var constructor = targetSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == propertySymbols.Count);
        if (constructor == null)
        {
            var constructorDiagnostic = Diagnostic.Create(
                DiagnosticDescriptors.InvalidConstructor,
                location,
                propertySymbols.Count,
                className
            );

            diagnostics.Add(constructorDiagnostic);

            return null;
        }

        var properties = new List<EntityProperty>();
        foreach (var propertySymbol in propertySymbols)
        {
            // find matching constructor name
            var parameter = constructor
                .Parameters
                .FirstOrDefault(p => string.Equals(p.Name, propertySymbol.Name, StringComparison.InvariantCultureIgnoreCase));

            if (parameter == null)
            {
                var constructorDiagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.InvalidConstructorParameter,
                    location,
                    propertySymbol.Name,
                    className
                );

                diagnostics.Add(constructorDiagnostic);

                continue;
            }

            var property = CreateProperty(propertySymbol, parameter.Name);
            properties.Add(property);
        }

        return new EntityClass(mode, fullyQualified, classNamespace, className, properties);
    }

    protected static List<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>();

        var currentSymbol = targetSymbol;

        // get nested properties
        while (currentSymbol != null)
        {
            var propertySymbols = currentSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(IsIncluded)
                .Where(p => !properties.ContainsKey(p.Name));

            foreach (var propertySymbol in propertySymbols)
                properties.Add(propertySymbol.Name, propertySymbol);

            currentSymbol = currentSymbol.BaseType;
        }

        return properties.Values.ToList();
    }

    protected static EntityProperty CreateProperty(IPropertySymbol propertySymbol, string parameterName = null)
    {
        var propertyType = propertySymbol.Type.ToDisplayString();
        var propertyName = propertySymbol.Name;

        // look for custom field converter
        var attributes = propertySymbol.GetAttributes();
        if (attributes == null || attributes.Length == 0)
        {
            return new EntityProperty(
                propertyName,
                propertyName,
                propertyType,
                parameterName);
        }

        var columnName = GetColumnName(attributes) ?? propertyName;

        var converter = attributes
            .FirstOrDefault(a => a.AttributeClass is
            {
                Name: "DataFieldConverterAttribute",
                ContainingNamespace.Name: "FluentCommand"
            });

        if (converter == null)
        {
            return new EntityProperty(
                propertyName,
                columnName,
                propertyType,
                parameterName);
        }

        // attribute contructor
        var converterType = converter.ConstructorArguments.FirstOrDefault();
        if (converterType.Value is INamedTypeSymbol converterSymbol)
        {
            return new EntityProperty(
                propertyName,
                columnName,
                propertyType,
                parameterName,
                converterSymbol.ToDisplayString());
        }

        // generic attribute
        var attributeClass = converter.AttributeClass;
        if (attributeClass is { IsGenericType: true }
            && attributeClass.TypeArguments.Length == attributeClass.TypeParameters.Length
            && attributeClass.TypeArguments.Length == 1)
        {
            var typeArgument = attributeClass.TypeArguments[0];
            var converterString = typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return new EntityProperty(
                propertyName,
                columnName,
                propertyType,
                parameterName,
                converterString);
        }

        return new EntityProperty(
            propertyName,
            columnName,
            propertyType,
            parameterName);
    }

    protected static string GetColumnName(ImmutableArray<AttributeData> attributes)
    {
        var columnAttribute = attributes
           .FirstOrDefault(a => a.AttributeClass is
           {
               Name: "ColumnAttribute",
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
           });

        if (columnAttribute == null)
            return null;

        // attribute contructor [Column("Name")]
        var converterType = columnAttribute.ConstructorArguments.FirstOrDefault();
        if (converterType.Value is string stringValue)
            return stringValue;

        return null;
    }

    protected static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        var attributes = propertySymbol.GetAttributes();
        if (attributes.Length > 0 && attributes.Any(
                a => a.AttributeClass is
                {
                    Name: "NotMappedAttribute",
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
                }))
        {
            return false;
        }

        return !propertySymbol.IsIndexer && !propertySymbol.IsAbstract && propertySymbol.DeclaredAccessibility == Accessibility.Public;
    }
}
