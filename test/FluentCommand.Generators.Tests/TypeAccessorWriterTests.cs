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
        Assert.Contains("typeof(string)", source);
        Assert.DoesNotContain("typeof(string?)", source);
        Assert.Contains("typeof(int?)", source);

        // nullable types use default pattern instead of throwing on null
        Assert.Contains("value is null ? default : (string?)value", source);
        Assert.Contains("value is null ? default : (int?)value", source);

        // non-nullable types throw ArgumentNullException on null
        Assert.Contains("throw new global::System.ArgumentNullException(nameof(value), \"Property 'Id' does not accept null.\")", source);
        Assert.Contains("throw new global::System.ArgumentNullException(nameof(value), \"Property 'Name' does not accept null.\")", source);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public void GetValueThrowsOnNullInstance()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Item",
            "FluentCommand.Entities",
            "Item",
            new EntityProperty[]
            {
                new("Id", "Id", "int", "int", IsKey: true),
            },
            TableName: "Item"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        // GetValue validates instance type via as-cast + null check
        Assert.Contains("var typed = instance as global::FluentCommand.Entities.Item;", source);
        Assert.Contains("throw new global::System.ArgumentException(\"Expected instance of type Item.\", nameof(instance));", source);
    }

    [Fact]
    public void SetValueThrowsOnNullInstance()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Item",
            "FluentCommand.Entities",
            "Item",
            new EntityProperty[]
            {
                new("Id", "Id", "int", "int", IsKey: true),
            },
            TableName: "Item"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        // SetValue validates instance type via as-cast + null check
        Assert.Contains("if (typed is null)", source);
        Assert.Contains("throw new global::System.ArgumentException(\"Expected instance of type Item.\", nameof(instance));", source);
    }

    [Fact]
    public void SetValueNonNullableRejectsNull()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Item",
            "FluentCommand.Entities",
            "Item",
            new EntityProperty[]
            {
                new("Id", "Id", "int", "int", IsKey: true),
                new("Name", "Name", "string", "string", IsRequired: true),
            },
            TableName: "Item"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        // non-nullable value type: throws ArgumentNullException
        Assert.Contains("throw new global::System.ArgumentNullException(nameof(value), \"Property 'Id' does not accept null.\")", source);

        // non-nullable reference type: also throws ArgumentNullException
        Assert.Contains("throw new global::System.ArgumentNullException(nameof(value), \"Property 'Name' does not accept null.\")", source);

        // both use direct cast without null-forgiving operator
        Assert.Contains("typed.Id = (int)value;", source);
        Assert.Contains("typed.Name = (string)value;", source);
        Assert.DoesNotContain("value!", source);
    }

    [Fact]
    public void SetValueNullableAcceptsNull()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Item",
            "FluentCommand.Entities",
            "Item",
            new EntityProperty[]
            {
                new("Age", "Age", "int?", "int?"),
                new("Email", "Email", "string?", "string"),
            },
            TableName: "Item"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        // nullable types use default-on-null pattern
        Assert.Contains("typed.Age = value is null ? default : (int?)value;", source);
        Assert.Contains("typed.Email = value is null ? default : (string?)value;", source);

        // no ArgumentNullException for nullable properties
        Assert.DoesNotContain("does not accept null", source);
    }

    [Fact]
    public void GetValueNoGetterThrowsInvalidOperation()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Item",
            "FluentCommand.Entities",
            "Item",
            new EntityProperty[]
            {
                new("Secret", "Secret", "string", "string", HasGetter: false),
            },
            TableName: "Item"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        Assert.Contains("throw new global::System.InvalidOperationException(\"Property 'Secret' does not have a getter.\")", source);
    }

    [Fact]
    public void SetValueNoSetterThrowsInvalidOperation()
    {
        var entityClass = new EntityClass(
            InitializationMode.ObjectInitializer,
            "global::FluentCommand.Entities.Item",
            "FluentCommand.Entities",
            "Item",
            new EntityProperty[]
            {
                new("ReadOnly", "ReadOnly", "string", "string", HasSetter: false),
            },
            TableName: "Item"
        );

        var source = TypeAccessorWriter.Generate(entityClass);

        Assert.Contains("throw new global::System.InvalidOperationException(\"Property 'ReadOnly' does not have a setter.\")", source);
    }
}
