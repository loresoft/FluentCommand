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
    public void BuildMergeTests()
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

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }


    [Fact]
    public void BuildMergeDataTests()
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

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }
    [Fact]
    public void BuildMergeDataTypeTests()
    {
        var definition = new DataMergeDefinition();

        DataMergeDefinition.AutoMap<DataType>(definition);
        definition.Columns.Should().NotBeNullOrEmpty();

        definition.TargetTable = "dbo.DataType";

        var column = definition.Columns.Find(c => c.SourceColumn == "Id");
        column.Should().NotBeNull();

        column.IsKey = true;
        column.CanUpdate = false;

        var users = new List<DataType>
        {
            new DataType
            {
                Id = 1,
                Name = "Test1",
                Boolean = false,
                Short = 2,
                Long = 200,
                Float = 200.20F,
                Double = 300.35,
                Decimal = 456.12M,
                DateTime = DateTime.Now,
                DateTimeOffset = DateTimeOffset.Now,
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
                DateTimeNull = DateTime.Now,
                DateTimeOffsetNull = DateTimeOffset.Now,
                GuidNull = Guid.Empty,
                TimeSpanNull = TimeSpan.FromHours(1),
                DateOnlyNull = new DateOnly(2022, 12, 1),
                TimeOnlyNull = new TimeOnly(1, 30, 0),
            },
            new DataType
            {
                Id = 2,
                Name = "Test2",
                Boolean = true,
                Short = 3,
                Long = 400,
                Float = 600.20F,
                Double = 700.35,
                Decimal = 856.12M,
                DateTime = DateTime.Now,
                DateTimeOffset = DateTimeOffset.Now,
                Guid = Guid.Empty,
                TimeSpan = TimeSpan.FromHours(2),
                DateOnly = new DateOnly(2022, 12, 12),
                TimeOnly = new TimeOnly(6, 30, 0),
            }
        };

        var listDataReader = new ListDataReader<DataType>(users);

        var sql = DataMergeGenerator.BuildMerge(definition, listDataReader);
        sql.Should().NotBeNullOrEmpty();

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }

    [Fact]
    public void BuildMergeDataOutputTests()
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
                EmailAddress = $"random.{DateTime.Now.Ticks}@email.com",
                DisplayName = "Random User",
                FirstName = "Random",
                LastName = "User"
            }
        };

        var dataTable = new ListDataReader<UserImport>(users);

        var sql = DataMergeGenerator.BuildMerge(definition, dataTable);
        sql.Should().NotBeNullOrEmpty();

        Output.WriteLine("MergeStatement:");
        Output.WriteLine(sql);
    }

}
