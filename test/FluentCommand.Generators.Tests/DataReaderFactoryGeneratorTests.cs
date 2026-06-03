using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

using FluentCommand.Attributes;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.Generators.Tests;

public class DataReaderFactoryGeneratorTests
{
    [Fact]
    public Task GenerateReaderAttributeOnClassGeneratesReader()
    {
        var source = """
            using FluentCommand.Attributes;

            namespace TestModels;

            [GenerateReader(typeof(Customer))]
            public static class CustomerReaderGeneration
            {
            }

            public sealed class Customer
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public bool IsActive { get; set; }
            }
            """;

        return Verify(source, "TestModels.CustomerDataReaderExtensions.g.cs");
    }

    [Fact]
    public Task GenerateReaderAttributeOptionsConfigureGeneratedProperties()
    {
        var source = """
            using System;
            using System.ComponentModel.DataAnnotations.Schema;
            using System.Text.Json;
            using FluentCommand.Attributes;

            [assembly: GenerateReader(
                typeof(TestModels.ExternalProduct),
                IgnoreProperties = new[] { nameof(TestModels.ExternalProduct.InternalState) },
                JsonProperties = new[] { nameof(TestModels.ExternalProduct.Metadata) },
                JsonOptionsProviderType = typeof(TestModels.ProductJsonOptionsProvider))]

            namespace TestModels;

            public sealed class ExternalProduct
            {
                public int Id { get; set; }

                [Column("display_name")]
                public string Name { get; set; } = "";

                public ProductMetadata Metadata { get; set; } = new();

                public object InternalState { get; set; } = new();
            }

            public sealed class ProductMetadata
            {
                public string Source { get; set; } = "";
            }

            public sealed class ProductJsonOptionsProvider
            {
                public static JsonSerializerOptions Options { get; } = new();
            }
            """;

        return Verify(source, "TestModels.ExternalProductDataReaderExtensions.g.cs");
    }

    [Fact]
    public Task GenerateReaderAttributeConstructorModeGeneratesReader()
    {
        var source = """
            using FluentCommand.Attributes;

            [assembly: GenerateReader(typeof(TestModels.InventoryItem))]

            namespace TestModels;

            public sealed class InventoryItem
            {
                public InventoryItem(int id, string sku, int quantity)
                {
                    Id = id;
                    Sku = sku;
                    Quantity = quantity;
                }

                public int Id { get; }
                public string Sku { get; }
                public int Quantity { get; }
            }
            """;

        return Verify(source, "TestModels.InventoryItemDataReaderExtensions.g.cs");
    }

    [Fact]
    public Task TableAttributeClassGeneratesReader()
    {
        var source = """
            using System;
            using System.ComponentModel.DataAnnotations.Schema;

            namespace TestModels;

            [Table("orders", Schema = "sales")]
            public sealed class Order
            {
                public int Id { get; set; }

                [Column("order_number")]
                public string Number { get; set; } = "";

                [NotMapped]
                public string DisplayName { get; set; } = "";
            }
            """;

        return Verify(source, "TestModels.OrderDataReaderExtensions.g.cs");
    }

    [Fact]
    public Task TableAttributeRecordGeneratesReader()
    {
        var source = """
            using System;
            using System.ComponentModel.DataAnnotations.Schema;

            namespace TestModels;

            [Table("audit_logs")]
            public sealed record AuditLog(
                int Id,
                [property: Column("event_name")] string EventName,
                DateTimeOffset Created);
            """;

        return Verify(source, "TestModels.AuditLogDataReaderExtensions.g.cs");
    }


    private static Task Verify(string source, string generatedFileName)
    {
        var output = GetGeneratedOutput<DataReaderFactoryGenerator>(source, generatedFileName);

        return Verifier
            .Verify(output)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    private static string GetGeneratedOutput<T>(string source, string generatedFileName = "DataReaderFactory.g.cs")
        where T : IIncrementalGenerator, new()
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
                MetadataReference.CreateFromFile(typeof(T).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GenerateReaderAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IServiceCollection).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TableAttribute).Assembly.Location),
            ]);

        var compilation = CSharpCompilation.Create(
            assemblyName: "Test.Generator",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var originalTreeCount = compilation.SyntaxTrees.Length;
        var generator = new T();

        var driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            parseOptions: parseOptions);

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var generated = outputCompilation.SyntaxTrees
            .Skip(originalTreeCount)
            .FirstOrDefault(t => Path.GetFileName(t.FilePath) == generatedFileName);

        return generated?.ToString() ?? string.Empty;
    }
}
