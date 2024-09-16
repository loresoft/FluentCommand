using System.Collections.Immutable;

using FluentCommand.Generators.Models;

namespace FluentCommand.Generators.Tests;

public class DataReaderFactoryWriterTests
{
    [Fact]
    public async Task Generate()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Status",
            "FluentCommand.Entities",
            "Status",
            new EntityProperty[]
            {
                new("Id", "Id", typeof(int).FullName),
                new("Name", "Name", typeof(string).FullName),
                new("IsActive", "IsActive", typeof(bool).FullName),
                new("Updated", "Updated", typeof(DateTimeOffset).FullName),
                new("RowVersion", "RowVersion", typeof(byte[]).FullName),
            }
        );

        var source = DataReaderFactoryWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }
}
