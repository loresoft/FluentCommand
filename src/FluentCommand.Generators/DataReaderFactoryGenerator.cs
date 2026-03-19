using System.Collections.Immutable;

using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

public abstract class DataReaderFactoryGenerator
{
    protected static void WriteDataReaderSource(SourceProductionContext context, EntityClass entityClass)
    {
        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = DataReaderFactoryWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}DataReaderExtensions.g.cs", source);
    }

    protected static void WriteTypeAccessorSource(SourceProductionContext context, EntityClass entityClass)
    {
        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = TypeAccessorWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}TypeAccessor.g.cs", source);
    }


    protected static EntityClass? CreateClass(INamedTypeSymbol targetSymbol)
    {
        if (targetSymbol == null)
            return null;

        var fullyQualified = targetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        // extract table mapping info
        var typeAttributes = targetSymbol.GetAttributes();
        var tableAttribute = FindSchemaAttribute(typeAttributes, "TableAttribute");

        string? tableName = null;
        string? tableSchema = null;

        if (tableAttribute != null)
        {
            if (tableAttribute.ConstructorArguments.Length > 0 && tableAttribute.ConstructorArguments[0].Value is string name)
                tableName = name;

            tableSchema = GetNamedString(tableAttribute, "Schema");
        }

        var mode = targetSymbol.Constructors.Any(c => c.Parameters.Length == 0)
            ? InitializationMode.ObjectInitializer
            : InitializationMode.Constructor;

        var propertySymbols = GetProperties(targetSymbol);

        if (mode == InitializationMode.ObjectInitializer)
        {
            var propertyArray = propertySymbols
                .Select(p => CreateProperty(p))
                .ToArray();

            return new EntityClass(
                InitializationMode: mode,
                FullyQualified: fullyQualified,
                EntityNamespace: classNamespace,
                EntityName: className,
                Properties: propertyArray,
                TableName: tableName,
                TableSchema: tableSchema
            );
        }

        // constructor initialization

        // constructor with same number of parameters as properties
        var constructor = targetSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == propertySymbols.Count);
        if (constructor == null)
            return null;

        var properties = new List<EntityProperty>();
        foreach (var propertySymbol in propertySymbols)
        {
            // find matching constructor name
            var parameter = constructor
                .Parameters
                .FirstOrDefault(p => string.Equals(p.Name, propertySymbol.Name, StringComparison.InvariantCultureIgnoreCase));

            if (parameter == null)
                continue;

            var property = CreateProperty(propertySymbol, parameter.Name);
            properties.Add(property);
        }

        return new EntityClass(
            InitializationMode: mode,
            FullyQualified: fullyQualified,
            EntityNamespace: classNamespace,
            EntityName: className,
            Properties: properties,
            TableName: tableName,
            TableSchema: tableSchema
        );
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

    protected static EntityProperty CreateProperty(IPropertySymbol propertySymbol, string? parameterName = null)
    {
        var propertyType = propertySymbol.Type.ToDisplayString();
        var memberTypeName = propertySymbol.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
        var propertyName = propertySymbol.Name;
        var hasGetter = propertySymbol.GetMethod != null;
        var hasSetter = propertySymbol.SetMethod != null && !propertySymbol.SetMethod.IsInitOnly;

        var attributes = propertySymbol.GetAttributes();
        if (attributes == default || attributes.Length == 0)
        {
            return new EntityProperty(
                PropertyName: propertyName,
                ColumnName: propertyName,
                PropertyType: propertyType,
                MemberTypeName: memberTypeName,
                ParameterName: parameterName,
                HasGetter: hasGetter,
                HasSetter: hasSetter
            );
        }

        var columnName = GetColumnName(attributes) ?? propertyName;
        var converterName = GetConverterName(attributes);

        var isKey = HasDataAnnotationAttribute(attributes, "KeyAttribute");
        var isNotMapped = IsNotMapped(attributes);
        var isDatabaseGenerated = GetIsDatabaseGenerated(attributes);
        var isConcurrencyCheck = HasDataAnnotationAttribute(attributes, "ConcurrencyCheckAttribute");
        var foreignKey = GetSchemaAttributeConstructorStringArg(attributes, "ForeignKeyAttribute");
        var isRequired = HasDataAnnotationAttribute(attributes, "RequiredAttribute");
        var displayName = GetNamedString(FindDataAnnotationAttribute(attributes, "DisplayAttribute"), "Name");
        var dataFormatString = GetNamedString(FindDataAnnotationAttribute(attributes, "DisplayFormatAttribute"), "DataFormatString");
        var columnType = GetNamedString(FindSchemaAttribute(attributes, "ColumnAttribute"), "TypeName");
        var columnOrder = GetNamedNumber(FindSchemaAttribute(attributes, "ColumnAttribute"), "Order");

        return new EntityProperty(
            PropertyName: propertyName,
            ColumnName: columnName,
            PropertyType: propertyType,
            MemberTypeName: memberTypeName,
            ParameterName: parameterName,
            ConverterName: converterName,
            IsKey: isKey,
            IsNotMapped: isNotMapped,
            IsDatabaseGenerated: isDatabaseGenerated,
            IsConcurrencyCheck: isConcurrencyCheck,
            ForeignKey: foreignKey,
            IsRequired: isRequired,
            HasGetter: hasGetter,
            HasSetter: hasSetter,
            DisplayName: displayName,
            DataFormatString: dataFormatString,
            ColumnType: columnType,
            ColumnOrder: columnOrder
        );
    }

    protected static string? GetColumnName(ImmutableArray<AttributeData> attributes)
    {
        var columnAttribute = FindSchemaAttribute(attributes, "ColumnAttribute");

        if (columnAttribute == null)
            return null;

        // attribute constructor [Column("Name")]
        var converterType = columnAttribute.ConstructorArguments.FirstOrDefault();
        if (converterType.Value is string stringValue)
            return stringValue;

        return null;
    }


    private static string? GetConverterName(ImmutableArray<AttributeData> attributes)
    {
        var converter = attributes
            .FirstOrDefault(a => a.AttributeClass is
            {
                Name: "DataFieldConverterAttribute",
                ContainingNamespace.Name: "FluentCommand"
            });

        if (converter == null)
            return null;

        // attribute constructor
        var converterType = converter.ConstructorArguments.FirstOrDefault();
        if (converterType.Value is INamedTypeSymbol converterSymbol)
            return converterSymbol.ToDisplayString();

        // generic attribute
        var attributeClass = converter.AttributeClass;
        if (attributeClass is { IsGenericType: true }
            && attributeClass.TypeArguments.Length == attributeClass.TypeParameters.Length
            && attributeClass.TypeArguments.Length == 1)
        {
            var typeArgument = attributeClass.TypeArguments[0];
            return typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        return null;
    }

    private static AttributeData? FindDataAnnotationAttribute(ImmutableArray<AttributeData> attributes, string name)
    {
        return attributes.FirstOrDefault(a => a.AttributeClass is
        {
            ContainingNamespace:
            {
                Name: "DataAnnotations",
                ContainingNamespace:
                {
                    Name: "ComponentModel",
                    ContainingNamespace.Name: "System"
                }
            }
        } && a.AttributeClass.Name == name);
    }

    private static bool HasDataAnnotationAttribute(ImmutableArray<AttributeData> attributes, string name)
    {
        return FindDataAnnotationAttribute(attributes, name) != null;
    }

    protected static AttributeData? FindSchemaAttribute(ImmutableArray<AttributeData> attributes, string name)
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

    private static string? GetSchemaAttributeConstructorStringArg(ImmutableArray<AttributeData> attributes, string name)
    {
        var attribute = FindSchemaAttribute(attributes, name);
        if (attribute?.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is string value)
            return value;

        return null;
    }

    private static bool GetIsDatabaseGenerated(ImmutableArray<AttributeData> attributes)
    {
        var attribute = FindSchemaAttribute(attributes, "DatabaseGeneratedAttribute");
        if (attribute == null)
            return false;

        if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is int option)
            return option != 0; // DatabaseGeneratedOption.None = 0

        return false;
    }

    private static string? GetNamedString(AttributeData? attribute, string argName)
    {
        if (attribute == null)
            return null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == argName && namedArg.Value.Value is string value)
                return value;
        }

        return null;
    }

    private static int? GetNamedNumber(AttributeData? attribute, string argName)
    {
        if (attribute == null)
            return null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == argName && namedArg.Value.Value is int value)
                return value;
        }

        return null;
    }

    protected static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        return !propertySymbol.IsIndexer && !propertySymbol.IsAbstract && propertySymbol.DeclaredAccessibility == Accessibility.Public;
    }

    private static bool IsNotMapped(ImmutableArray<AttributeData> attributes)
    {
        return attributes.Any(
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
            });
    }
}
