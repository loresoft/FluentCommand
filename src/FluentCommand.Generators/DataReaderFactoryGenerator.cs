using System.Collections.Immutable;

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

        // Emit the diagnostic, if needed
        var diagnostics = provider
            .Select(static (item, _) => item.Diagnostics)
            .Where(static item => !item.IsDefaultOrEmpty);

        context.RegisterSourceOutput(diagnostics, ReportDiagnostic);

        var entityClasses = provider
            .Select(static (item, _) => item.EntityClass)
            .Where(static item => item is not null);

        context.RegisterSourceOutput(entityClasses, Execute);
    }

    private static void ReportDiagnostic(SourceProductionContext context, ImmutableArray<Diagnostic> diagnostics)
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
        return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclaration && !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
            || syntaxNode is RecordDeclarationSyntax { AttributeLists.Count: > 0 } recordDeclaration && !recordDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword);
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
                .Select(p => new EntityProperty(p.Name, p.Type.ToDisplayString()))
                .ToImmutableArray();

            return new EntityContext(new EntityClass(mode, classNamespace, className, propertyArray), ImmutableArray<Diagnostic>.Empty);
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

            return new EntityContext(null, diagnostics.ToImmutableArray());
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

            var property = new EntityProperty(propertySymbol.Name, propertySymbol.Type.ToDisplayString(), parameter.Name);
            properties.Add(property);
        }

        return new EntityContext(new EntityClass(mode, classNamespace, className, properties.ToImmutableArray()), diagnostics.ToImmutableArray());
    }

    public static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        var attributes = propertySymbol.GetAttributes();
        if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"))
            return false;

        return !propertySymbol.IsIndexer && !propertySymbol.IsAbstract && propertySymbol.DeclaredAccessibility == Accessibility.Public;
    }
}
