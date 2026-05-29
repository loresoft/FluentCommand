using FluentCommand.Generators.Models;

namespace FluentCommand.Generators.Tests;

public class TypeAccessorWriterTests
{
    [Fact]
    public async Task GenerateObjectInitializer()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Status",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Status",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = typeof(int).FullName!, MemberTypeName = typeof(int).FullName!, IsKey = true, IsDatabaseGenerated = true },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = typeof(string).FullName!, MemberTypeName = typeof(string).FullName!, IsRequired = true },
                new() { PropertyName = "IsActive", ColumnName = "IsActive", PropertyType = typeof(bool).FullName!, MemberTypeName = typeof(bool).FullName! },
                new() { PropertyName = "Updated", ColumnName = "Updated", PropertyType = typeof(DateTimeOffset).FullName!, MemberTypeName = typeof(DateTimeOffset).FullName!, IsConcurrencyCheck = true },
                new() { PropertyName = "RowVersion", ColumnName = "RowVersion", PropertyType = typeof(byte[]).FullName!, MemberTypeName = typeof(byte[]).FullName! },
            },
            TableName = "Status",
            TableSchema = "dbo"
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateWithColumnMapping()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.User",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "User",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "user_id", PropertyType = typeof(int).FullName!, MemberTypeName = typeof(int).FullName!, IsKey = true, IsDatabaseGenerated = true, ColumnType = "int" },
                new() { PropertyName = "Name", ColumnName = "user_name", PropertyType = typeof(string).FullName!, MemberTypeName = typeof(string).FullName!, IsRequired = true, DisplayName = "User Name" },
                new() { PropertyName = "Email", ColumnName = "Email", PropertyType = typeof(string).FullName!, MemberTypeName = typeof(string).FullName! },
            },
            TableName = "Users"
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateConstructorMode()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.Constructor,
            FullyQualified = "global::FluentCommand.Entities.UserRecord",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "UserRecord",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = typeof(int).FullName!, MemberTypeName = typeof(int).FullName!, ParameterName = "id", IsKey = true },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = typeof(string).FullName!, MemberTypeName = typeof(string).FullName!, ParameterName = "name" },
            }
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public async Task GenerateNullableReferenceType()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Contact",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Contact",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "int", MemberTypeName = "int", IsKey = true, IsDatabaseGenerated = true },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = "string", MemberTypeName = "string", IsRequired = true },
                new() { PropertyName = "Email", ColumnName = "Email", PropertyType = "string?", MemberTypeName = "string", IsNullable = true },
                new() { PropertyName = "Age", ColumnName = "Age", PropertyType = "int?", MemberTypeName = "int?", IsNullable = true },
            },
            TableName = "Contact"
        };

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
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Item",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Item",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "int", MemberTypeName = "int", IsKey = true },
            },
            TableName = "Item"
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        // GetValue validates instance type via as-cast + null check
        Assert.Contains("var typed = instance as global::FluentCommand.Entities.Item;", source);
        Assert.Contains("throw new global::System.ArgumentException(\"Expected instance of type Item.\", nameof(instance));", source);
    }

    [Fact]
    public void SetValueThrowsOnNullInstance()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Item",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Item",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "int", MemberTypeName = "int", IsKey = true },
            },
            TableName = "Item"
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        // SetValue validates instance type via as-cast + null check
        Assert.Contains("if (typed is null)", source);
        Assert.Contains("throw new global::System.ArgumentException(\"Expected instance of type Item.\", nameof(instance));", source);
    }

    [Fact]
    public void SetValueNonNullableRejectsNull()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Item",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Item",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "int", MemberTypeName = "int", IsKey = true },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = "string", MemberTypeName = "string", IsRequired = true },
            },
            TableName = "Item"
        };

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
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Item",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Item",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Age", ColumnName = "Age", PropertyType = "int?", MemberTypeName = "int?", IsNullable = true },
                new() { PropertyName = "Email", ColumnName = "Email", PropertyType = "string?", MemberTypeName = "string", IsNullable = true },
            },
            TableName = "Item"
        };

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
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Item",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Item",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Secret", ColumnName = "Secret", PropertyType = "string", MemberTypeName = "string", HasGetter = false },
            },
            TableName = "Item"
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        Assert.Contains("throw new global::System.InvalidOperationException(\"Property 'Secret' does not have a getter.\")", source);
    }

    [Fact]
    public void SetValueNoSetterThrowsInvalidOperation()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Item",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Item",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "ReadOnly", ColumnName = "ReadOnly", PropertyType = "string", MemberTypeName = "string", HasSetter = false },
            },
            TableName = "Item"
        };

        var source = TypeAccessorWriter.Generate(entityClass);

        Assert.Contains("throw new global::System.InvalidOperationException(\"Property 'ReadOnly' does not have a setter.\")", source);
    }
}
