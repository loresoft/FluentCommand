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
        var entityClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "System.ComponentModel.DataAnnotations.Schema.TableAttribute",
            predicate: SyntacticPredicate,
            transform: SemanticTransform
        )
        .Where(static context => context is not null)
        .Select(static (context, _) => context!)
        .WithTrackingName("TableAttributeGenerator");

        context.RegisterSourceOutput(entityClasses, WriteDataReaderSource);
        context.RegisterSourceOutput(entityClasses, WriteTypeAccessorSource);
    }

    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
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
    }

    private static EntityClass? SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol targetSymbol)
            return null;

        return CreateClass(targetSymbol);
    }
}
