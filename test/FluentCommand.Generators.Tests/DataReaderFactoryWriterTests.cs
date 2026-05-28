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
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = typeof(int).FullName!, MemberTypeName = typeof(int).FullName! },
                new() { PropertyName = "Name", ColumnName = "Name", PropertyType = typeof(string).FullName!, MemberTypeName = typeof(string).FullName! },
                new() { PropertyName = "IsActive", ColumnName = "IsActive", PropertyType = typeof(bool).FullName!, MemberTypeName = typeof(bool).FullName! },
                new() { PropertyName = "Updated", ColumnName = "Updated", PropertyType = typeof(DateTimeOffset).FullName!, MemberTypeName = typeof(DateTimeOffset).FullName! },
                new() { PropertyName = "RowVersion", ColumnName = "RowVersion", PropertyType = typeof(byte[]).FullName!, MemberTypeName = typeof(byte[]).FullName! },
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
                new() { PropertyName = "Id", ColumnName = "Id", PropertyType = "int", MemberTypeName = "int" },
                new() { PropertyName = "Data", ColumnName = "Data", PropertyType = "global::FluentCommand.Entities.UserImport", MemberTypeName = "global::FluentCommand.Entities.UserImport", IsJsonColumn = true },
                new() { PropertyName = "DataWithOptions", ColumnName = "DataWithOptions", PropertyType = "global::FluentCommand.Entities.UserImport", MemberTypeName = "global::FluentCommand.Entities.UserImport", IsJsonColumn = true, JsonOptionsProviderName = "global::FluentCommand.Entities.UserImportJsonOptionsProvider" },
                new() { PropertyName = "DataWithContext", ColumnName = "DataWithContext", PropertyType = "global::FluentCommand.Entities.UserImport", MemberTypeName = "global::FluentCommand.Entities.UserImport", IsJsonColumn = true, JsonContextName = "global::FluentCommand.Entities.UserImportJsonContext", JsonTypeInfoPropertyName = "UserImport" },
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        Assert.Contains("v_data = dataRecord.GetFromJson<global::FluentCommand.Entities.UserImport>(__index);", source);
        Assert.Contains("v_dataWithOptions = dataRecord.GetFromJson<global::FluentCommand.Entities.UserImport>(__index, global::FluentCommand.Entities.UserImportJsonOptionsProvider.Options);", source);
        Assert.Contains("v_dataWithContext = dataRecord.GetFromJson<global::FluentCommand.Entities.UserImport>(__index, global::FluentCommand.Entities.UserImportJsonContext.Default.UserImport);", source);
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
                    EnumUnderlyingType = "short"
                },
                new()
                {
                    PropertyName = "OptionalStatus",
                    ColumnName = "OptionalStatus",
                    PropertyType = "global::FluentCommand.Entities.BuilderStatus?",
                    MemberTypeName = "global::FluentCommand.Entities.BuilderStatus",
                    IsEnum = true,
                    IsNullableEnum = true,
                    EnumUnderlyingType = "short"
                }
            }
        };

        var source = DataReaderFactoryWriter.Generate(entityClass);

        Assert.Contains("v_status = (global::FluentCommand.Entities.BuilderStatus)dataRecord.GetInt16(__index);", source);
        Assert.Contains("v_optionalStatus = (global::FluentCommand.Entities.BuilderStatus?)dataRecord.GetValue<short?>(__index);", source);
    }
}
