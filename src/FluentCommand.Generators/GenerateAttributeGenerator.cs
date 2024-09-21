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
            .SelectMany(static (item, _) => item.EntityClasses)
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

        var classes = new List<EntityClass>();
        var diagnostics = new List<Diagnostic>();

        foreach (var attribute in context.Attributes)
        {
            if (attribute == null)
                return null;

            if (attribute.ConstructorArguments.Length != 1)
                return null;

            var comparerArgument = attribute.ConstructorArguments[0];
            if (comparerArgument.Value is not INamedTypeSymbol targetSymbol)
                return null;

            var entityClass = CreateClass(context.TargetNode.GetLocation(), targetSymbol, diagnostics);
            if (entityClass != null)
                classes.Add(entityClass);
        }

        return new EntityContext(classes, diagnostics);
    }
}
