using FluentCommand.Generators.Models;

namespace FluentCommand.Generators.Tests;

public class DataReaderFactoryWriterTests
{
    [Fact]
    public async Task Generate()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Status",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Status",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "global::System.Int32", MemberTypeName = "global::System.Int32" },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = "global::System.String", MemberTypeName = "global::System.String" },
                new() { PropertyName = "IsActive", ColumnName = "IsActive", PropertyType = "global::System.Boolean", MemberTypeName = "global::System.Boolean" },
                new() { PropertyName = "Updated", ColumnName = "Updated", PropertyType = "global::System.DateTimeOffset", MemberTypeName = "global::System.DateTimeOffset" },
                new() { PropertyName = "RowVersion", ColumnName = "RowVersion", PropertyType = "global::System.Byte[]", MemberTypeName = "global::System.Byte[]" },
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        await Verifier
            .Verify(source)
            .UseDirectory("Snapshots")
            .ScrubLinesContaining("GeneratedCodeAttribute");
    }

    [Fact]
    public void GenerateJsonColumnReader()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.UserLog",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "UserLog",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "global::System.Int32", MemberTypeName = "global::System.Int32" },
                new() { PropertyName = "Data", ColumnName = "Data", PropertyType = "global::FluentCommand.Entities.UserImport", MemberTypeName = "global::FluentCommand.Entities.UserImport", IsJsonColumn = true },
                new() { PropertyName = "OptionalData", ColumnName = "OptionalData", PropertyType = "global::FluentCommand.Entities.UserImport?", MemberTypeName = "global::FluentCommand.Entities.UserImport?", IsJsonColumn = true, IsNullable = true },
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        Assert.Contains("global::System.Text.Json.JsonSerializerOptions? jsonSerializerOptions", source);
        Assert.Contains("dataRecord is global::FluentCommand.IDataReaderContext __context", source);
        Assert.Contains("v_data = dataRecord.GetRequiredFromJson<global::FluentCommand.Entities.UserImport>(__index, jsonSerializerOptions);", source);
        Assert.Contains("v_optionalData = dataRecord.GetFromJson<global::FluentCommand.Entities.UserImport?>(__index, jsonSerializerOptions);", source);
        Assert.Contains("Factory(r, jsonSerializerOptions)", source);
    }

    [Fact]
    public void GenerateNonJsonColumnReaderDoesNotGenerateJsonOptionsOverloads()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.Status",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "Status",
            Properties = new EntityProperty[]
            {
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "global::System.Int32", MemberTypeName = "global::System.Int32" },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = "global::System.String", MemberTypeName = "global::System.String" },
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        Assert.DoesNotContain("JsonSerializerOptions? jsonSerializerOptions", source);
        Assert.DoesNotContain("IDataReaderContext", source);
    }

    [Fact]
    public void GenerateEnumColumnReader()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.EnumLog",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "EnumLog",
            Properties = new EntityProperty[]
            {
                new()
                {
                    PropertyName = "Status",
                    ColumnName = "Status",
                    PropertyType = "global::FluentCommand.Entities.BuilderStatus",
                    MemberTypeName = "global::FluentCommand.Entities.BuilderStatus",
                    IsEnum = true,
                    EnumUnderlyingType = "global::System.Int16"
                },
                new()
                {
                    PropertyName = "OptionalStatus",
                    ColumnName = "OptionalStatus",
                    PropertyType = "global::FluentCommand.Entities.BuilderStatus?",
                    MemberTypeName = "global::FluentCommand.Entities.BuilderStatus",
                    IsEnum = true,
                    IsNullable = true,
                    IsNullableEnum = true,
                    EnumUnderlyingType = "global::System.Int16"
                }
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        Assert.Contains("v_status = (global::FluentCommand.Entities.BuilderStatus)dataRecord.GetInt16(__index);", source);
        Assert.Contains("v_optionalStatus = (global::FluentCommand.Entities.BuilderStatus?)dataRecord.GetValue<short?>(__index);", source);
    }

    [Fact]
    public void GenerateConverterColumnReader()
    {
        var entityClass = new EntityClass
        {
            InitializationMode = InitializationMode.ObjectInitializer,
            FullyQualified = "global::FluentCommand.Entities.ConverterLog",
            EntityNamespace = "FluentCommand.Entities",
            EntityName = "ConverterLog",
            Properties = new EntityProperty[]
            {
                new()
                {
                    PropertyName = "Value",
                    ColumnName = "Value",
                    PropertyType = "global::FluentCommand.Entities.CustomValue",
                    MemberTypeName = "global::FluentCommand.Entities.CustomValue",
                    ConverterName = "global::FluentCommand.Entities.CustomValueConverter"
                }
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        Assert.Contains("var c_value = global::FluentCommand.Internal.Singleton<global::FluentCommand.Entities.CustomValueConverter>.Current", source);
        Assert.Contains("as global::FluentCommand.IDataFieldConverter<global::FluentCommand.Entities.CustomValue>;", source);
    }
}
