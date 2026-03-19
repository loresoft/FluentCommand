using FluentCommand.Generators.Models;

namespace FluentCommand.Generators.Tests;

public class TypeAccessorWriterTests
{
    [Fact]
    public async Task GenerateObjectInitializer()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Status",
            "FluentCommand.Entities",
            "Status",
            new EntityProperty[]
            {
                new("Id", "Id", typeof(int).FullName!, typeof(int).FullName!, IsKey: true, IsDatabaseGenerated: true),
                new("Name", "Name", typeof(string).FullName!, typeof(string).FullName!, IsRequired: true),
                new("IsActive", "IsActive", typeof(bool).FullName!, typeof(bool).FullName!),
                new("Updated", "Updated", typeof(DateTimeOffset).FullName!, typeof(DateTimeOffset).FullName!, IsConcurrencyCheck: true),
                new("RowVersion", "RowVersion", typeof(byte[]).FullName!, typeof(byte[]).FullName!),
            },
            TableName: "Status",
            TableSchema: "dbo"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateWithColumnMapping()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.User",
            "FluentCommand.Entities",
            "User",
            new EntityProperty[]
            {
                new("Id", "user_id", typeof(int).FullName!, typeof(int).FullName!, IsKey: true, IsDatabaseGenerated: true, ColumnType: "int"),
                new("Name", "user_name", typeof(string).FullName!, typeof(string).FullName!, IsRequired: true, DisplayName: "User Name"),
                new("Email", "Email", typeof(string).FullName!, typeof(string).FullName!),
            },
            TableName: "Users"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateConstructorMode()
    {
        var entityClass = new EntityClass(
            InitializationMode.Constructor,
            "global::FluentCommand.Entities.UserRecord",
            "FluentCommand.Entities",
            "UserRecord",
            new EntityProperty[]
            {
                new("Id", "Id", typeof(int).FullName!, typeof(int).FullName!, ParameterName: "id", IsKey: true),
                new("Name", "Name", typeof(string).FullName!, typeof(string).FullName!, ParameterName: "name"),
            }
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateNullableReferenceType()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Contact",
            "FluentCommand.Entities",
            "Contact",
            new EntityProperty[]
            {
                new("Id", "Id", "int", "int", IsKey: true, IsDatabaseGenerated: true),
                new("Name", "Name", "string", "string", IsRequired: true),
                new("Email", "Email", "string?", "string"),
                new("Age", "Age", "int?", "int?"),
            },
            TableName: "Contact"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        // verify typeof() uses MemberTypeName (no nullable annotation)
        // and setter cast uses PropertyType (preserving nullable annotation)
        Assert.Contains("typeof(string)", source);
        Assert.DoesNotContain("typeof(string?)", source);
        Assert.Contains("typeof(int?)", source);
        Assert.Contains("= (string?)value!", source);
        Assert.Contains("= (int?)value!", source);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }
}
