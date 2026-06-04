using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;

using FluentCommand.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.Generators.Tests;

public class DataReaderFactoryDiagnosticTests
{
    [Fact]
    public async Task AnalyzerReportsNoMatchingConstructor()
    {
        var source = """
            using FluentCommand.Attributes;

            [assembly: GenerateReader(typeof(TestModels.Product))]

            namespace TestModels;

            public sealed class Product
            {
                public Product(int id)
                {
                    Id = id;
                }

                public int Id { get; }
                public string Name { get; }
            }
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        AssertDiagnosticCount(diagnostics, "FLC001", 1);
    }

    [Fact]
    public async Task AnalyzerReportsConstructorParameterNotMatched()
    {
        var source = """
            using FluentCommand.Attributes;

            [assembly: GenerateReader(typeof(TestModels.Product))]

            namespace TestModels;

            public sealed class Product
            {
                public Product(int key, string name)
                {
                    Id = key;
                    Name = name;
                }

                public int Id { get; }
                public string Name { get; }
            }
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        AssertDiagnosticCount(diagnostics, "FLC002", 1);
    }

    [Fact]
    public async Task AnalyzerReportsNoMappableProperties()
    {
        var source = """
            using FluentCommand.Attributes;

            [assembly: GenerateReader(typeof(TestModels.Product))]

            namespace TestModels;

            public sealed class Product
            {
                public object Metadata { get; set; } = new();
            }
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        AssertDiagnosticCount(diagnostics, "FLC003", 1);
        AssertDiagnosticCount(diagnostics, "FLC004", 1);
    }

    [Fact]
    public async Task AnalyzerReportsUnsupportedPropertyType()
    {
        var source = """
            using FluentCommand.Attributes;

            [assembly: GenerateReader(typeof(TestModels.Product))]

            namespace TestModels;

            public sealed class Product
            {
                public int Id { get; set; }
                public ProductMetadata Metadata { get; set; } = new();
            }

            public sealed class ProductMetadata;
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        AssertDiagnosticCount(diagnostics, "FLC004", 1);
        Assert.DoesNotContain(diagnostics, static d => d.Id == "FLC003");
    }

    [Fact]
    public async Task AnalyzerReportsInvalidGenerateReaderArgument()
    {
        var source = """
            using FluentCommand.Attributes;

            [assembly: GenerateReader(null)]
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        AssertDiagnosticCount(diagnostics, "FLC005", 1);
    }

    [Fact]
    public async Task AnalyzerReportsTableAttributeOnInvalidTypes()
    {
        var source = """
            using System.ComponentModel.DataAnnotations.Schema;

            namespace TestModels;

            [Table("static_products")]
            public static class StaticProduct
            {
            }

            [Table("abstract_products")]
            public abstract class AbstractProduct
            {
                public int Id { get; set; }
            }
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        AssertDiagnosticCount(diagnostics, "FLC006", 2);
    }

    [Fact]
    public async Task AnalyzerReportsInvalidGenerateReaderOptions()
    {
        var source = """
            using System;
            using FluentCommand.Attributes;

            [assembly: GenerateReader(
                typeof(TestModels.ExternalProduct),
                IgnoreProperties = new[] { "MissingIgnore" },
                JsonProperties = new[] { "MissingJson", nameof(TestModels.ExternalProduct.Metadata) })]

            namespace TestModels;

            public sealed class ExternalProduct
            {
                public int Id { get; set; }
                public ProductMetadata Metadata { get; set; } = new();
            }

            public sealed class ProductMetadata;
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.Equal(2, diagnostics.Count(static d => d.Id == "FLC007"));
    }

    [Fact]
    public async Task AnalyzerUsesGenerateReaderOptionsForConstructorMappableCount()
    {
        var source = """
            using System;
            using FluentCommand.Attributes;

            [assembly: GenerateReader(
                typeof(TestModels.ExternalProduct),
                IgnoreProperties = new[] { nameof(TestModels.ExternalProduct.InternalState) },
                JsonProperties = new[] { nameof(TestModels.ExternalProduct.Metadata) })]

            namespace TestModels;

            public sealed class ExternalProduct
            {
                public ExternalProduct(int id, ProductMetadata metadata)
                {
                    Id = id;
                    Metadata = metadata;
                }

                public int Id { get; }
                public ProductMetadata Metadata { get; }
                public object InternalState { get; } = new();
            }

            public sealed class ProductMetadata;
            """;

        var diagnostics = await GetAnalyzerDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, static d => d.Id == "FLC001");
        Assert.DoesNotContain(diagnostics, static d => d.Id == "FLC004");
    }


    private static void AssertDiagnosticCount(ImmutableArray<Diagnostic> diagnostics, string id, int expectedCount)
    {
        Assert.Equal(expectedCount, diagnostics.Count(d => d.Id == id));
    }

    private static async Task<ImmutableArray<Diagnostic>> GetAnalyzerDiagnosticsAsync(string source)
    {
        var compilation = CreateCompilation(source);
        var analyzer = new DataReaderFactoryAnalyzer();

        return await compilation
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols(
            "FLUENTCOMMAND_GENERATOR",
            "NET7_0_OR_GREATER",
            "NET8_0_OR_GREATER",
            "NET9_0_OR_GREATER",
            "NET10_0_OR_GREATER");

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(DataReaderFactoryAnalyzer).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GenerateReaderAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TableAttribute).Assembly.Location),
            ]);

        return CSharpCompilation.Create(
            assemblyName: "Test.Analyzer",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
