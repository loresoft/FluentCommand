using FluentCommand.Entities;
using FluentCommand.Merge;

using System.Text.Json;

namespace FluentCommand.SqlServer.Tests;

public class DataMergeGeneratorTests
{
    private class JsonItem
    {
        public int Id { get; set; }
        public JsonElement Payload { get; set; }
    }

    public DataMergeGeneratorTests(ITestOutputHelper output)
    {
        Output = output;
    }

    public ITestOutputHelper Output { get; }

    [Fact]
    public void ParseIdentifier()
    {
        string result = DataMergeGenerator.ParseIdentifier("[Name]");
        result.Should().Be("Name");

        result = DataMergeGenerator.ParseIdentifier("[Name");
        result.Should().Be("[Name");

        result = DataMergeGenerator.ParseIdentifier("Name]");
        result.Should().Be("Name]");

        result = DataMergeGenerator.ParseIdentifier("[Nam]e]");
        result.Should().Be("Nam]e");

        result = DataMergeGenerator.ParseIdentifier("[]");
        result.Should().Be("");

        result = DataMergeGenerator.ParseIdentifier("B");
        result.Should().Be("B");

        result = DataMergeGenerator.ParseIdentifier("[Nam]]e]");
        result.Should().Be("Nam]e");
    }

    [Fact]
    public void QuoteIdentifier()
    {
        string result = DataMergeGenerator.QuoteIdentifier("[Name]");
        result.Should().Be("[Name]");

        result = DataMergeGenerator.QuoteIdentifier("Name");
        result.Should().Be("[Name]");

        result = DataMergeGenerator.QuoteIdentifier("[Name");
        result.Should().Be("[[Name]");

        result = DataMergeGenerator.QuoteIdentifier("Name]");
        result.Should().Be("[Name]]]");

        result = DataMergeGenerator.QuoteIdentifier("Nam]e");
        result.Should().Be("[Nam]]e]");

        result = DataMergeGenerator.QuoteIdentifier("");
        result.Should().Be("[]");

        result = DataMergeGenerator.QuoteIdentifier("B");
        result.Should().Be("[B]");

        result = DataMergeGenerator.QuoteIdentifier("[Nam]]e]");
        result.Should().Be("[Nam]]e]");

        result = DataMergeGenerator.QuoteIdentifier("[x]; DROP TABLE y; --[z]");
        result.Should().Be("[[x]]; DROP TABLE y; --[z]]]");

        result = DataMergeGenerator.QuoteIdentifier("[a]]");
        result.Should().Be("[[a]]]]]");
    }

    [Fact]
    public void TableIdentifierInjection()
    {
        var result = DataMergeGenerator.TableIdentifier("dbo.[x]; DROP TABLE y; --[z]");
        result.Should().Be("[dbo].[[x]]; DROP TABLE y; --[z]]]");
    }

    [Fact]
    public void BuildTableWithoutColumnsThrows()
    {
        var definition = new DataMergeDefinition { TargetTable = "dbo.Item" };
        definition.Columns.Add(new DataMergeColumn { SourceColumn = "Id", TargetColumn = "Id", NativeType = "int", IsIgnored = true });

        var action = () => DataMergeGenerator.BuildTable(definition);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BuildMergeWithoutActionThrows()
    {
        var definition = CreateObjectDefinition();
        definition.IncludeInsert = false;
        definition.IncludeUpdate = false;
        definition.IncludeDelete = false;

        var action = () => DataMergeGenerator.BuildMerge(definition);

        action.Should().Throw<InvalidOperationException>().WithMessage("*insert, update, or delete*");
    }

    [Fact]
    public void BuildMergeIdentityInsertResetsOnError()
    {
        var definition = CreateObjectDefinition();
        definition.IdentityInsert = true;

        var sql = DataMergeGenerator.BuildMerge(definition);

        sql.Should().StartWith("SET IDENTITY_INSERT [dbo].[Item] ON;");
        sql.Should().Contain("BEGIN TRY");
        sql.Should().Contain("BEGIN CATCH\r\n    SET IDENTITY_INSERT [dbo].[Item] OFF;\r\n    THROW;\r\nEND CATCH;".ReplaceLineEndings());
        sql.TrimEnd().Should().EndWith("SET IDENTITY_INSERT [dbo].[Item] OFF;");
    }

    [Fact]
    public void BuildMergeDataObjectColumnIsQuoted()
    {
        var definition = CreateObjectDefinition();

        var table = new System.Data.DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Value", typeof(object));
        table.Rows.Add(1, "x'); DROP TABLE y; --");
        table.Rows.Add(2, new DateTime(2024, 5, 13, 8, 30, 15));
        table.Rows.Add(3, 42);

        using var reader = table.CreateDataReader();

        var sql = DataMergeGenerator.BuildMerge(definition, reader);

        sql.Should().Contain("(1, N'x''); DROP TABLE y; --')");
        sql.Should().Contain("(2, CAST('2024-05-13 08:30:15.0000000' AS datetime2))");
        sql.Should().Contain("(3, 42)");
    }

    [Fact]
    public void BuildMergeDataUnicodeString()
    {
        var definition = CreateObjectDefinition();

        var table = new System.Data.DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Value", typeof(string));
        table.Rows.Add(1, "Привет 日本");

        using var reader = table.CreateDataReader();

        var sql = DataMergeGenerator.BuildMerge(definition, reader);

        sql.Should().Contain("(1, N'Привет 日本')");
    }

    [Fact]
    public void BuildMergeDataTimeSpanOutOfRangeThrows()
    {
        var definition = CreateObjectDefinition();

        var table = new System.Data.DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Value", typeof(TimeSpan));
        table.Rows.Add(1, TimeSpan.FromHours(26));

        using var reader = table.CreateDataReader();

        var action = () => DataMergeGenerator.BuildMerge(definition, reader);

        action.Should().Throw<InvalidOperationException>().WithMessage("*outside the range*");
    }

    [Fact]
    public void BuildMergeDataMissingColumnThrows()
    {
        var definition = CreateObjectDefinition();

        var table = new System.Data.DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Rows.Add(1);

        using var reader = table.CreateDataReader();

        var action = () => DataMergeGenerator.BuildMerge(definition, reader);

        action.Should().Throw<InvalidOperationException>().WithMessage("*missing*Value*");
    }

    [Fact]
    public void BuildMergeDataUsesMergeColumnName()
    {
        var definition = CreateObjectDefinition();

        var table = new System.Data.DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("VALUE", typeof(int));
        table.Rows.Add(1, 2);

        using var reader = table.CreateDataReader();

        var sql = DataMergeGenerator.BuildMerge(definition, reader);

        sql.Should().Contain("[Id], [Value]");
    }

    private static DataMergeDefinition CreateObjectDefinition()
    {
        var definition = new DataMergeDefinition { TargetTable = "dbo.Item" };
        definition.Columns.Add(new DataMergeColumn { SourceColumn = "Id", TargetColumn = "Id", NativeType = "int", IsKey = true, CanUpdate = false });
        definition.Columns.Add(new DataMergeColumn { SourceColumn = "Value", TargetColumn = "Value", NativeType = "sql_variant" });

        return definition;
    }

    [Fact]
    public void BuildMergeSoftDeleteBoolean()
    {
        var definition = DataMergeDefinition.Create<UserImport>();
        definition.TargetTable = "dbo.User";
        definition.IncludeDelete = true;
        definition.SoftDeleteColumn = "IsDeleted";
        definition.SoftDeleteValue = true;
        definition.Columns.Find(c => c.SourceColumn == "EmailAddress")!.IsKey = true;

        var sql = DataMergeGenerator.BuildMerge(definition);

        sql.Should().Contain("WHEN NOT MATCHED BY SOURCE AND (t.[IsDeleted] IS NULL OR t.[IsDeleted] <> 1) THEN \r\n    UPDATE SET t.[IsDeleted] = 1".ReplaceLineEndings());
    }

    [Fact]
    public void BuildMergeSoftDeleteString()
    {
        var definition = DataMergeDefinition.Create<UserImport>();
        definition.TargetTable = "dbo.User";
        definition.IncludeDelete = true;
        definition.SoftDeleteColumn = "Status";
        definition.SoftDeleteValue = "O'Deleted";
        definition.Columns.Find(c => c.SourceColumn == "EmailAddress")!.IsKey = true;

        var sql = DataMergeGenerator.BuildMerge(definition);

        sql.Should().Contain("UPDATE SET t.[Status] = N'O''Deleted'");
    }

    [Fact]
    public void BuildMergeHardDeleteWithoutSoftDeleteColumn()
    {
        var definition = DataMergeDefinition.Create<UserImport>();
        definition.TargetTable = "dbo.User";
        definition.IncludeDelete = true;
        definition.Columns.Find(c => c.SourceColumn == "EmailAddress")!.IsKey = true;

        var sql = DataMergeGenerator.BuildMerge(definition);

        sql.Should().Contain("WHEN NOT MATCHED BY SOURCE THEN \r\n    DELETE".ReplaceLineEndings());
    }

    [Fact]
    public void BuildMergeNoDeleteWhenIncludeDeleteFalse()
    {
        var definition = DataMergeDefinition.Create<UserImport>();
        definition.TargetTable = "dbo.User";
        definition.IncludeDelete = false;
        definition.SoftDeleteColumn = "IsDeleted";
        definition.SoftDeleteValue = true;
        definition.Columns.Find(c => c.SourceColumn == "EmailAddress")!.IsKey = true;

        var sql = DataMergeGenerator.BuildMerge(definition);

        sql.Should().NotContain("NOT MATCHED BY SOURCE");
    }

    [Fact]
    public async System.Threading.Tasks.Task BuildMergeTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<UserImport>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.TargetTable = "dbo.User";

        var column = definition.Columns.Find(c => c.SourceColumn == "EmailAddress");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var sql = DataMergeGenerator.BuildMerge(definition);
        sql.Should().NotBeNullOrEmpty();

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots")
            .AddScrubber(scrubber => scrubber.Replace(definition.TemporaryTable, "#MergeTable"));
    }


    [Fact]
    public async System.Threading.Tasks.Task BuildMergeDataTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<UserImport>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.TargetTable = "dbo.User";

        var column = definition.Columns.Find(c => c.SourceColumn == "EmailAddress");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var users = new List<UserImport>
        {
            new UserImport
            {
                EmailAddress = "test@email.com",
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User"
            },
            new UserImport
            {
                EmailAddress = "blah@email.com",
                DisplayName = "Blah User",
                FirstName = "Blah",
                LastName = "User"
            }
        };

        var listDataReader = new ListDataReader<UserImport>(users);

        var sql = DataMergeGenerator.BuildMerge(definition, listDataReader);
        sql.Should().NotBeNullOrEmpty();

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task BuildTableSqlTest()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<DataType>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();
        definition.TargetTable = "dbo.DataType";

        var column = definition.Columns.Find(c => c.SourceColumn == "Id");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var tableStatement = DataMergeGenerator.BuildTable(definition);
        tableStatement.Should().NotBeNull();
        await Verifier
            .Verify(tableStatement)
            .UseDirectory("Snapshots")
            .AddScrubber(scrubber => scrubber.Replace(definition.TemporaryTable, "#MergeTable"));

    }

    [Fact]
    public async System.Threading.Tasks.Task BuildMergeDataTypeTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<DataType>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();
        definition.TargetTable = "dbo.DataType";

        var column = definition.Columns.Find(c => c.SourceColumn == "Id");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var items = new List<DataType>
        {
            new() {
                Id = 1,
                Name = "Test1",
                Boolean = false,
                Short = 2,
                Long = 200,
                Float = 200.20F,
                Double = 300.35,
                Decimal = 456.12M,
                DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
                DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
                Guid = Guid.Empty,
                TimeSpan = TimeSpan.FromHours(1),
                DateOnly = new DateOnly(2022, 12, 1),
                TimeOnly = new TimeOnly(1, 30, 0),
                BooleanNull = false,
                ShortNull = 2,
                LongNull = 200,
                FloatNull = 200.20F,
                DoubleNull = 300.35,
                DecimalNull = 456.12M,
                DateTimeNull = new DateTime(2024, 4, 1, 8, 0, 0),
                DateTimeOffsetNull = new DateTimeOffset(2024, 4, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
                GuidNull = Guid.Empty,
                TimeSpanNull = TimeSpan.FromHours(1),
                DateOnlyNull = new DateOnly(2022, 12, 1),
                TimeOnlyNull = new TimeOnly(1, 30, 0),
            },
            new() {
                Id = 2,
                Name = "Test2",
                Boolean = true,
                Short = 3,
                Long = 400,
                Float = 600.20F,
                Double = 700.35,
                Decimal = 856.12M,
                DateTime = new DateTime(2024, 5, 1, 8, 0, 0),
                DateTimeOffset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-6)),
                Guid = Guid.Empty,
                TimeSpan = TimeSpan.FromHours(2),
                DateOnly = new DateOnly(2022, 12, 12),
                TimeOnly = new TimeOnly(6, 30, 0),
            }
        };

        var listDataReader = new ListDataReader<DataType>(items);

        var mergeDataStatement = DataMergeGenerator.BuildMerge(definition, listDataReader);
        mergeDataStatement.Should().NotBeNullOrEmpty();
        await Verifier
            .Verify(mergeDataStatement)
            .UseDirectory("Snapshots")
            .AddScrubber(scrubber => scrubber.Replace(definition.TemporaryTable, "#MergeTable"));
    }

    [Fact]
    public async System.Threading.Tasks.Task BuildMergeDataTableTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<DataType>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();
        definition.TargetTable = "dbo.DataType";

        var column = definition.Columns.Find(c => c.SourceColumn == "Id");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var mergeStatement = DataMergeGenerator.BuildMerge(definition);
        mergeStatement.Should().NotBeNull();
        await Verifier
            .Verify(mergeStatement)
            .UseDirectory("Snapshots")
            .AddScrubber(scrubber => scrubber.Replace(definition.TemporaryTable, "#MergeTable"));
    }

    [Fact]
    public async System.Threading.Tasks.Task BuildMergeDataOutputTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<UserImport>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.IncludeOutput = true;
        definition.TargetTable = "dbo.User";

        var column = definition.Columns.Find(c => c.SourceColumn == "EmailAddress");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var users = new List<UserImport>
        {
            new UserImport
            {
                EmailAddress = "test@email.com",
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User"
            },
            new UserImport
            {
                EmailAddress = "blah@email.com",
                DisplayName = "Blah User",
                FirstName = "Blah",
                LastName = "User"
            },
            new UserImport
            {
                EmailAddress = $"random@email.com",
                DisplayName = "Random User",
                FirstName = "Random",
                LastName = "User"
            }
        };

        var dataTable = new ListDataReader<UserImport>(users);

        var sql = DataMergeGenerator.BuildMerge(definition, dataTable);
        sql.Should().NotBeNullOrEmpty();

        await Verifier
            .Verify(sql)
            .UseDirectory("Snapshots");
    }

    [Fact]
    public async System.Threading.Tasks.Task BuildMergeDataMismatchTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<Member>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        var column = definition.Columns.Find(c => c.SourceColumn == "email_address");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var users = new List<Member>
        {
            new Member
            {
                EmailAddress = "test@email.com",
                DisplayName = "Test User",
                FirstName = "Test",
                LastName = "User"
            },
            new Member
            {
                EmailAddress = "blah@email.com",
                DisplayName = "Blah User",
                FirstName = "Blah",
                LastName = "User"
            },
            new Member
            {
                EmailAddress = $"random@email.com",
                DisplayName = "Random User",
                FirstName = "Random",
                LastName = "User"
            }
        };

        var dataTable = new ListDataReader<Member>(users);

        var mergeDataStatement = DataMergeGenerator.BuildMerge(definition, dataTable);
        mergeDataStatement.Should().NotBeNullOrEmpty();

        await Verifier
            .Verify(mergeDataStatement)
            .UseDirectory("Snapshots");
    }

    [Fact]
    public void BuildMergeDataJsonElementTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<JsonItem>(definition);
        definition.TargetTable = "dbo.JsonItem";

        var keyColumn = definition.Columns.Find(c => c.SourceColumn == "Id");
        keyColumn.Should().NotBeNull();

        keyColumn.IsKey = true;
        keyColumn.CanUpdate = false;

        var payloadColumn = definition.Columns.Find(c => c.SourceColumn == "Payload");
        payloadColumn.Should().NotBeNull();
        payloadColumn.NativeType.Should().Be("nvarchar(MAX)");
        payloadColumn.IsIgnored.Should().BeFalse();

        using var document = JsonDocument.Parse("""{"name":"Test's","active":true}""");
        var items = new List<JsonItem>
        {
            new() { Id = 1, Payload = document.RootElement.Clone() }
        };

        var dataTable = new ListDataReader<JsonItem>(items);

        var mergeDataStatement = DataMergeGenerator.BuildMerge(definition, dataTable);

        mergeDataStatement.Should().Contain("""N'{"name":"Test''s","active":true}'""");
    }

    [Fact]
    public void BuildMergeDataUndefinedJsonElementIsNull()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<JsonItem>(definition);
        definition.TargetTable = "dbo.JsonItem";

        var keyColumn = definition.Columns.Find(c => c.SourceColumn == "Id");
        keyColumn.Should().NotBeNull();

        keyColumn.IsKey = true;
        keyColumn.CanUpdate = false;

        var items = new List<JsonItem>
        {
            new() { Id = 1, Payload = default }
        };

        var dataTable = new ListDataReader<JsonItem>(items);

        var mergeDataStatement = DataMergeGenerator.BuildMerge(definition, dataTable);

        mergeDataStatement.Should().Contain("(1, NULL)");
    }
}
