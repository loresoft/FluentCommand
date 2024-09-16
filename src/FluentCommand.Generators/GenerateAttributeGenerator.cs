using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

[Generator(LanguageNames.CSharp)]
public class GenerateAttributeGenerator : DataReaderFactoryGenerator, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "FluentCommand.Attributes.GenerateReaderAttribute",
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

        context.RegisterSourceOutput(entityClasses, WriteSource);
    }

    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return true;
    }

    private static EntityContext SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Attributes.Length == 0)
            return null;

        var attribute = context.Attributes[0];
        if (attribute == null)
            return null;

        if (attribute.ConstructorArguments.Length != 1)
            return null;

        var comparerArgument = attribute.ConstructorArguments[0];
        if (comparerArgument.Value is not INamedTypeSymbol targetSymbol)
            return null;

        return CreateContext(context.TargetNode.GetLocation(), targetSymbol);
    }
}
