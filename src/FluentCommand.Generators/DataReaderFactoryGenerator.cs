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
        );

        context.RegisterSourceOutput(provider, Execute);
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

    private static EntityClass SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var classNamespace = targetSymbol.ContainingNamespace.ToDisplayString();
        var className = targetSymbol.Name;

        var properties = targetSymbol
            .GetMembers()
            .Where(m => m.Kind == SymbolKind.Property)
            .OfType<IPropertySymbol>()
            .Where(IsIncluded)
            .Select(p => new EntityProperty(p.Name, p.Type.ToDisplayString()))
            .ToImmutableArray();

        return new EntityClass(InitializationMode.ObjectInitializer, classNamespace, className, properties);
    }

    public static bool IsIncluded(IPropertySymbol propertySymbol)
    {
        var attributes = propertySymbol.GetAttributes();
        if (attributes.Any(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"))
            return false;

        return !propertySymbol.IsIndexer && !propertySymbol.IsAbstract;
    }
}
