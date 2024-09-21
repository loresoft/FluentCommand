using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentCommand.Generators;

[Generator(LanguageNames.CSharp)]
public class TableAttributeGenerator : DataReaderFactoryGenerator, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "System.ComponentModel.DataAnnotations.Schema.TableAttribute",
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
            .SelectMany(static (item, _) => item.EntityClasses)
            .Where(static item => item is not null);

        context.RegisterSourceOutput(entityClasses, WriteSource);
    }

    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return (syntaxNode is ClassDeclarationSyntax
        { AttributeLists.Count: > 0 } classDeclaration
               && !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
               && !classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            || (syntaxNode is RecordDeclarationSyntax
            { AttributeLists.Count: > 0 } recordDeclaration
               && !recordDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
               && !recordDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword));
    }

    private static EntityContext SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        var classes = new List<EntityClass>();
        var diagnostics = new List<Diagnostic>();

        var entityClass = CreateClass(context.TargetNode.GetLocation(), targetSymbol, diagnostics);
        classes.Add(entityClass);

        return new EntityContext(classes, diagnostics);
    }
}
