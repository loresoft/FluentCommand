using FluentCommand.Entities;
using FluentCommand.Merge;

namespace FluentCommand.SqlServer.Tests;

public class DataMergeGeneratorTests
{
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
}
