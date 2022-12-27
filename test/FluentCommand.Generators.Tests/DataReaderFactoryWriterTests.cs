using System.Collections.Immutable;

using FluentCommand.Generators.Models;

namespace FluentCommand.Generators.Tests;

[UsesVerify]
public class DataReaderFactoryWriterTests
{
    [Fact]
    public async Task Generate()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "FluentCommand.Entities",
            "Status",
            new EntityProperty[]
            {
                new("Id", typeof(int).FullName),
                new("Name", typeof(string).FullName),
                new("IsActive", typeof(bool).FullName),
                new("Updated", typeof(DateTimeOffset).FullName),
                new("RowVersion", typeof(byte[]).FullName),
            }.ToImmutableArray()
        );

        var source = DataReaderFactoryWriter.Generate(entityClass, true);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots");
    }
}
