using System.Text.Json;
using System.Text.Json.Serialization;

using FluentCommand.Entities;
using FluentCommand.Import;

using Microsoft.Extensions.DependencyInjection;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SqlServer.Tests;

public class ImportProcessorTests : ImportProcessor
{
    public ImportProcessorTests()
        : base(null, new ServiceCollection().BuildServiceProvider())
    {
    }

    [Fact]
    public void CreateTableTest()
    {
        var userDefinition = CreateDefinition();
        userDefinition.Should().NotBeNull();
        userDefinition.Name.Should().Be("User");
        userDefinition.Fields.Count.Should().Be(5);

        var importData = CreateImportData();
        importData.Should().NotBeNull();

        var importContext = new ImportProcessContext(new ServiceCollection().BuildServiceProvider(), userDefinition, importData, "test@email.com");
        importContext.Should().NotBeNull();
        importContext.Definition.Should().NotBeNull();
        importContext.ImportData.Should().NotBeNull();

        var dataTable = this.CreateTable(importContext);
        dataTable.Should().NotBeNull();
        dataTable.Columns.Count.Should().Be(5);
        dataTable.Columns[0].ColumnName.Should().Be("EmailAddress");
        dataTable.Columns[0].DataType.Should().Be<string>();
        dataTable.Columns[1].ColumnName.Should().Be("FirstName");
        dataTable.Columns[1].DataType.Should().Be<string>();
        dataTable.Columns[2].ColumnName.Should().Be("LastName");
        dataTable.Columns[2].DataType.Should().Be<string>();
        dataTable.Columns[3].ColumnName.Should().Be("IsValidated");
        dataTable.Columns[3].DataType.Should().Be<bool>();
    }

    [Fact]
    public async Task CreateAndPopulateTable()
    {
        var userDefinition = CreateDefinition();
        userDefinition.Should().NotBeNull();
        userDefinition.Name.Should().Be("User");
        userDefinition.Fields.Count.Should().Be(5);

        var importData = CreateImportData();
        importData.Should().NotBeNull();

        var importContext = new ImportProcessContext(new ServiceCollection().BuildServiceProvider(), userDefinition, importData, "test@email.com");
        importContext.Should().NotBeNull();
        importContext.Definition.Should().NotBeNull();
        importContext.ImportData.Should().NotBeNull();


        var dataTable = this.CreateTable(importContext);
        dataTable.Should().NotBeNull();

        dataTable.Columns.Count.Should().Be(5);
        dataTable.Columns[0].ColumnName.Should().Be("EmailAddress");
        dataTable.Columns[0].DataType.Should().Be<string>();

        dataTable.Columns[1].ColumnName.Should().Be("FirstName");
        dataTable.Columns[1].DataType.Should().Be<string>();

        dataTable.Columns[2].ColumnName.Should().Be("LastName");
        dataTable.Columns[2].DataType.Should().Be<string>();

        dataTable.Columns[3].ColumnName.Should().Be("IsValidated");
        dataTable.Columns[3].DataType.Should().Be<bool>();

        dataTable.Columns[4].ColumnName.Should().Be("LockoutCount");
        dataTable.Columns[4].DataType.Should().Be<int>();

        await PopulateTable(importContext, dataTable);

        dataTable.Rows.Count.Should().Be(3);

        dataTable.Rows[0][0].Should().Be("user1@email.com");
        dataTable.Rows[0][1].Should().Be("first1");
        dataTable.Rows[0][2].Should().Be("last1");
        dataTable.Rows[0][3].Should().Be(true);
        dataTable.Rows[0][4].Should().Be(DBNull.Value);

        dataTable.Rows[1][0].Should().Be("user2@email.com");
        dataTable.Rows[1][1].Should().Be("first2");
        dataTable.Rows[1][2].Should().Be("");
        dataTable.Rows[1][3].Should().Be(false);
        dataTable.Rows[1][4].Should().Be(DBNull.Value);

        dataTable.Rows[2][0].Should().Be("user3@email.com");
        dataTable.Rows[2][1].Should().Be("first3");
        dataTable.Rows[2][2].Should().Be("last3");
        dataTable.Rows[2][3].Should().Be(false);
        dataTable.Rows[2][4].Should().Be(2);

    }

    [Fact]
    public void ImportDataJson()
    {
        var importData = CreateImportData();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(importData, options);
        json.Should().NotBeNullOrWhiteSpace();

        var deserialized = JsonSerializer.Deserialize<ImportData>(json, options);
        deserialized.Should().NotBeNull();
        deserialized.FileName.Should().Be(importData.FileName);
        deserialized.Mappings.Should().HaveCount(importData.Mappings.Count);
        deserialized.Data.Should().HaveSameCount(importData.Data);
        deserialized.Data[0].Should().BeEquivalentTo(importData.Data[0]);
    }

    [Fact]
    public void ImportDefinitionJson()
    {
        var importDefinition = CreateDefinition();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        var json = JsonSerializer.Serialize(importDefinition, options);
        json.Should().NotBeNullOrWhiteSpace();

        var deserialized = JsonSerializer.Deserialize<ImportDefinition>(json, options);
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(importDefinition.Name);
        deserialized.Fields.Should().HaveCount(importDefinition.Fields.Count);
        deserialized.Fields[0].Name.Should().Be(importDefinition.Fields[0].Name);
    }

    [Theory]
    [MemberData(nameof(ConvertData))]
    public async Task ConvertValueTest(string value, Type type, object expected)
    {
        var fieldDefinition = new FieldDefinition();
        fieldDefinition.Name = "Test";
        fieldDefinition.DataType = type;

        var convertedValue = await ConvertValue(null, fieldDefinition, value);
        Assert.Equal(expected, convertedValue);
    }

    [Theory]
    [InlineData(FieldDefault.UserName, null, "test@user.com")]
    [InlineData(FieldDefault.Static, "testing123", "testing123")]
    [InlineData(FieldDefault.Static, 42, 42)]
    public void GetDefaultTest(FieldDefault fieldDefault, object defaultValue, object expected)
    {
        var fieldDefinition = new FieldDefinition();
        fieldDefinition.Name = "Test";
        fieldDefinition.Default = fieldDefault;
        fieldDefinition.DefaultValue = defaultValue;

        var resultValue = GetDefault(fieldDefinition, "test@user.com");
        Assert.Equal(resultValue, expected);
    }

    [Fact]
    public void ImportDefinitionBuilderAutoMapAndFieldTest()
    {
        // AutoMap test
        var autoMappedDefinition = ImportDefinitionBuilder<User>.Build(b => b.AutoMap());

        autoMappedDefinition.Should().NotBeNull();
        autoMappedDefinition.Name.Should().Be(nameof(User));
        autoMappedDefinition.Fields.Should().NotBeNullOrEmpty();
        autoMappedDefinition.Fields.Any(f => f.Name == nameof(User.Id)).Should().BeTrue();
        autoMappedDefinition.Fields.Any(f => f.Name == nameof(User.EmailAddress)).Should().BeTrue();

        // Field<TValue> test
        var customDefinition = ImportDefinitionBuilder<User>.Build(b => b
            .Field(t => t.EmailAddress)
            .DisplayName("User Email")
            .Required()
        );

        customDefinition.Should().NotBeNull();
        var emailField = customDefinition.Fields.FirstOrDefault(f => f.Name == nameof(User.EmailAddress));
        emailField.Should().NotBeNull();
        emailField.DisplayName.Should().Be("User Email");
    }

    public static TheoryData<string, Type, object> ConvertData =>
        new()
        {
            { "1", typeof(int), 1 },
            { "true", typeof(bool), true },
            { "", typeof(int?), null },
            { "", typeof(int), 0 },
            { "test", typeof(string), "test" },
            { "3/5/2024", typeof(DateOnly), new DateOnly(2024, 3, 5) },
            { "2024-03-05", typeof(DateOnly), new DateOnly(2024, 3, 5) },
            { "", typeof(DateOnly), DateOnly.MinValue },
            { "", typeof(DateOnly?), null },
        };


    private static ImportData CreateImportData()
    {
        var importData = new ImportData();
        importData.FileName = "Testing.csv";
        importData.Mappings = new List<FieldMap>
        {
            new FieldMap {Name = "EmailAddress", Index = 0},
            new FieldMap {Name = "IsValidated", Index = 1},
            new FieldMap {Name = "LastName", Index = 2},
            new FieldMap {Name = "FirstName", Index = 3},
            new FieldMap {Name = "LockoutCount", Index = 4},
        };
        importData.Data = new[]
        {
            new[] {"EmailAddress", "IsValidated", "LastName", "FirstName", "LockoutCount"},
            new[] {"user1@email.com", "true", "last1", "first1", ""},
            new[] {"user2@email.com", "false", "", "first2", ""},
            new[] {"user3@email.com", "", "last3", "first3", "2"},
        };
        return importData;
    }

    private static ImportDefinition CreateDefinition()
    {
        var userDefinition = ImportDefinition.Build(b => b
            .Name("User")
            .Field(f => f
                .DisplayName("Email Address")
                .FieldName("EmailAddress")
                .DataType<string>()
                .Required()
            )
            .Field(f => f
                .DisplayName("First Name")
                .FieldName("FirstName")
                .DataType<string>()
            )
            .Field(f => f
                .DisplayName("Last Name")
                .FieldName("LastName")
                .DataType<string>()
            )
            .Field(f => f
                .DisplayName("Validated")
                .FieldName("IsValidated")
                .DataType<bool>()
            )
            .Field(f => f
                .DisplayName("Lockouts")
                .FieldName("LockoutCount")
                .DataType<int?>()
            )
        );

        return userDefinition;
    }


}
