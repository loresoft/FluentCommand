using FluentCommand.Generators.Models;

using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators;

[Generator(LanguageNames.CSharp)]
public class GenerateAttributeGenerator : DataReaderFactoryGenerator, IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "FluentCommand.Attributes.GenerateReaderAttribute",
            predicate: SyntacticPredicate,
            transform: SemanticTransform
        )
        .Where(static context => context.Count > 0)
        .SelectMany(static (item, _) => item)
        .WithTrackingName("GenerateAttributeGenerator");

        context.RegisterSourceOutput(entityClasses, WriteDataReaderSource);
        context.RegisterSourceOutput(entityClasses, WriteTypeAccessorSource);
    }

    private static bool SyntacticPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return true;
    }

    private static EquatableArray<EntityClass> SemanticTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Attributes.Length == 0)
            return EquatableArray<EntityClass>.Empty;

        var classes = new List<EntityClass>();

        foreach (var attribute in context.Attributes)
        {
            if (attribute == null)
                return EquatableArray<EntityClass>.Empty;

            if (attribute.ConstructorArguments.Length != 1)
                return EquatableArray<EntityClass>.Empty;

            var comparerArgument = attribute.ConstructorArguments[0];
            if (comparerArgument.Value is not INamedTypeSymbol targetSymbol)
                return EquatableArray<EntityClass>.Empty;

            var entityClass = CreateClass(targetSymbol);
            if (entityClass != null)
                classes.Add(entityClass);
        }

        return classes;
    }
}
