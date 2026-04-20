using System.Collections.Immutable;

using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentCommand.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class DataReaderFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Pipeline for [GenerateReader(typeof(T))] attribute
        var generateAttributeClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "FluentCommand.Attributes.GenerateReaderAttribute",
            predicate: static (_, __) => true,
            transform: static (context, _) =>
            {
                if (context.Attributes.Length == 0)
                    return [];

                var classes = new List<EntityClass>();

                foreach (var attribute in context.Attributes)
                {
                    if (attribute == null)
                        return [];

                    if (attribute.ConstructorArguments.Length != 1)
                        return [];

                    var comparerArgument = attribute.ConstructorArguments[0];
                    if (comparerArgument.Value is not INamedTypeSymbol targetSymbol)
                        return [];

                    var entityClass = CreateClass(targetSymbol);
                    if (entityClass != null)
                        classes.Add(entityClass);
                }

                return new EquatableArray<EntityClass>(classes);
            }
        )
        .Where(static context => context.Count > 0)
        .SelectMany(static (item, _) => item)
        .WithTrackingName("GenerateAttributeGenerator");

        context.RegisterSourceOutput(generateAttributeClasses, WriteDataReaderSource);
        context.RegisterSourceOutput(generateAttributeClasses, WriteTypeAccessorSource);

        // Pipeline for [Table] attribute
        var tableAttributeClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "System.ComponentModel.DataAnnotations.Schema.TableAttribute",
            predicate: static (syntaxNode, _) =>
            {
                return
                    (
                        syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclaration
                            && !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
                            && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                    )
                    ||
                    (
                        syntaxNode is RecordDeclarationSyntax { AttributeLists.Count: > 0 } recordDeclaration
                            && !recordDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
                            && !recordDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
                    );
            },
            transform: static (context, _) =>
            {
                if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
                    return null;

                return CreateClass(targetSymbol);
            }
        )
        .Where(static context => context is not null)
        .Select(static (context, _) => context!)
        .WithTrackingName("TableAttributeGenerator");

        context.RegisterSourceOutput(tableAttributeClasses, WriteDataReaderSource);
        context.RegisterSourceOutput(tableAttributeClasses, WriteTypeAccessorSource);
    }

    private static void WriteDataReaderSource(SourceProductionContext context, EntityClass entityClass)
    {
        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = DataReaderFactoryWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}DataReaderExtensions.g.cs", source);
    }

    private static void WriteTypeAccessorSource(SourceProductionContext context, EntityClass entityClass)
    {
        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = TypeAccessorWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}TypeAccessor.g.cs", source);
    }


    private static EntityClass? CreateClass(INamedTypeSymbol targetSymbol)
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

        var classIgnored = GetClassIgnoredProperties(typeAttributes);
        var propertySymbols = GetProperties(targetSymbol);

        if (mode == InitializationMode.ObjectInitializer)
        {
            var propertyArray = propertySymbols
                .Select(p => CreateProperty(p, classIgnored: classIgnored))
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

        // constructor with same number of parameters as mappable properties
        var mappableCount = propertySymbols
            .Count(p => !classIgnored.Contains(p.Name) && !HasIgnorePropertyAttribute(p.GetAttributes()));

        var constructor = targetSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == mappableCount);
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

            var property = CreateProperty(propertySymbol, parameter.Name, classIgnored: classIgnored);
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

    private static List<IPropertySymbol> GetProperties(INamedTypeSymbol targetSymbol)
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

        return [.. properties.Values];
    }

    private static EntityProperty CreateProperty(IPropertySymbol propertySymbol, string? parameterName = null, HashSet<string>? classIgnored = null)
    {
        var propertyType = propertySymbol.Type.ToDisplayString();
        var memberTypeName = propertySymbol.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
        var propertyName = propertySymbol.Name;
        var hasGetter = propertySymbol.GetMethod != null;
        var hasSetter = propertySymbol.SetMethod?.IsInitOnly == false;
        var isNotMapped = (classIgnored?.Contains(propertyName) == true) || !IsSupportedType(propertySymbol.Type);

        var attributes = propertySymbol.GetAttributes();
        if (attributes == default || attributes.Length == 0)
        {
            return new EntityProperty(
                PropertyName: propertyName,
                ColumnName: propertyName,
                PropertyType: propertyType,
                MemberTypeName: memberTypeName,
                ParameterName: parameterName,
                IsNotMapped: isNotMapped,
                HasGetter: hasGetter,
                HasSetter: hasSetter
            );
        }

        var columnName = GetColumnName(attributes) ?? propertyName;
        var converterName = GetConverterName(attributes);

        var isKey = HasDataAnnotationAttribute(attributes, "KeyAttribute");

        isNotMapped = isNotMapped
            || IsNotMapped(attributes)
            || HasIgnorePropertyAttribute(attributes);

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

    private static string? GetColumnName(ImmutableArray<AttributeData> attributes)
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

    private static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        return !propertySymbol.IsIndexer
            && !propertySymbol.IsAbstract
            && propertySymbol.DeclaredAccessibility == Accessibility.Public;
    }

    private static bool IsSupportedType(ITypeSymbol type)
    {
        // handle nullable value types
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
            return IsSupportedType(namedType.TypeArguments[0]);

        // enums are stored as their underlying integer type
        if (type.TypeKind == TypeKind.Enum)
            return true;

        // primitives and string
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

        // byte[]
        if (type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte })
            return true;

        // well-known struct types and FluentCommand.ConcurrencyToken
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

            // constructor argument: [IgnoreProperty("Name")] or [IgnoreProperty(nameof(T.Name))]
            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string ctorName)
            {
                ignored.Add(ctorName);
                continue;
            }

            // named argument: [IgnoreProperty(PropertyName = "Name")]
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "PropertyName" && namedArg.Value.Value is string namedValue)
                    ignored.Add(namedValue);
            }
        }

        return ignored;
    }
}
