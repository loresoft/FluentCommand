using System.Collections.Immutable;

using FluentCommand.Generators.Internal;
using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentCommand.Generators;

[Generator(LanguageNames.CSharp)]
public class DataReaderFactoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "FluentCommand.GenerateDataReaderAttribute",
            predicate: SyntacticPredicate,
            transform: SemanticTransform
        )
        .Where(static context => context is not null);

        // Emit the diagnostics, if needed
        var diagnostics = provider
            .Select(static (item, _) => item.Diagnostics)
            .Where(static item => item.Count > 0);

        context.RegisterSourceOutput(diagnostics, ReportDiagnostic);

        var entityClasses = provider
            .Select(static (item, _) => item.EntityClass)
            .Where(static item => item is not null);

        context.RegisterSourceOutput(entityClasses, Execute);
    }

    private static void ReportDiagnostic(SourceProductionContext context, EquatableArray<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            context.ReportDiagnostic(diagnostic);
    }

    private static void Execute(SourceProductionContext context, EntityClass entityClass)
    {
        var qualifiedName = entityClass.EntityNamespace is null
            ? entityClass.EntityName
            : $"{entityClass.EntityNamespace}.{entityClass.EntityName}";

        var source = DataReaderFactoryWriter.Generate(entityClass);

        context.AddSource($"{qualifiedName}.DataReaderFactory.g.cs", source);
    }

    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is ClassDeclarationSyntax
               { AttributeLists.Count: > 0 } classDeclaration
               && !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
               && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            || syntaxNode is RecordDeclarationSyntax
               { AttributeLists.Count: > 0 } recordDeclaration
               && !recordDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
               && !recordDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static EntityContext SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        var mode = targetSymbol.Constructors.Any(c => c.Parameters.Length == 0)
            ? InitializationMode.ObjectInitializer
            : InitializationMode.Constructor;

        var propertySymbols = targetSymbol
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .OfType<IPropertySymbol>()
            .Where(IsIncluded)
            .ToList();

        if (mode == InitializationMode.ObjectInitializer)
        {
            var propertyArray = propertySymbols
                .Select(p => CreateProperty(p));

            var entity = new EntityClass(mode, classNamespace, className, propertyArray);
            return new EntityContext(entity, Enumerable.Empty<Diagnostic>());
        }

        // constructor initialization
        var diagnostics = new List<Diagnostic>();

        // constructor with same number of parameters as properties
        var constructor = targetSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == propertySymbols.Count);
        if (constructor == null)
        {
            var constructorDiagnostic = Diagnostic.Create(
                DiagnosticDescriptors.InvalidConstructor,
                context.TargetNode.GetLocation(),
                propertySymbols.Count,
                className
            );

            diagnostics.Add(constructorDiagnostic);

            return new EntityContext(null, diagnostics);
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
                    context.TargetNode.GetLocation(),
                    propertySymbol.Name,
                    className
                );

                diagnostics.Add(constructorDiagnostic);

                continue;
            }

            var property = CreateProperty(propertySymbol, parameter.Name);
            properties.Add(property);
        }

        var entityClass = new EntityClass(mode, classNamespace, className, properties);
        return new EntityContext(entityClass, diagnostics);
    }

    private static EntityProperty CreateProperty(IPropertySymbol propertySymbol, string parameterName = null)
    {
        // look for custom field converter
        var attributes = propertySymbol.GetAttributes();
        if (attributes == null || attributes.Length == 0)
            return new EntityProperty(propertySymbol.Name, propertySymbol.Type.ToDisplayString(), parameterName);

        var converter = attributes
            .FirstOrDefault(a => a.AttributeClass is
            {
                Name: "DataFieldConverterAttribute",
                ContainingNamespace.Name: "FluentCommand"
            });

        if (converter == null)
            return new EntityProperty(propertySymbol.Name, propertySymbol.Type.ToDisplayString(), parameterName);

        var converterType = converter.ConstructorArguments.Single();
        if (converterType.Value is INamedTypeSymbol converterSymbol)
            return new EntityProperty(
                propertySymbol.Name,
                propertySymbol.Type.ToDisplayString(),
                parameterName,
                converterSymbol.ToDisplayString());

        return new EntityProperty(propertySymbol.Name, propertySymbol.Type.ToDisplayString(), parameterName);
    }

    private static bool IsIncluded(IPropertySymbol propertySymbol)
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
