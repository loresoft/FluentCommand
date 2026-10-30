using Bogus;

using FluentCommand.Entities;
using FluentCommand.Merge;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataMergeTests : DatabaseTestBase
{
    public DataMergeTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
    }

    [Fact]
    public void ExecuteTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        var result = session
            .MergeData("dbo.User")
            .Map<UserImport>(m => m
                .AutoMap()
                .Column(p => p.EmailAddress).Key()
            )
            .Execute(users);

        result.Should().Be(100);
    }

    [Fact]
    public void ExecuteOutputTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        var result = session
            .MergeData("dbo.User")
            .Map<UserImport>(m => m
                .AutoMap()
                .Column(p => p.EmailAddress).Key()
            )
            .ExecuteOutput(users)
            .ToList();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExecuteBulkCopyTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        using var session = Services.GetRequiredService<IDataSession>();
        var result = session
            .MergeData("dbo.User")
            .Mode(DataMergeMode.BulkCopy)
            .Map<UserImport>(m =>
            {
                m.Column(p => p.EmailAddress)
                    .NativeType("nvarchar(256)")
                    .Key();

                m.Column(p => p.DisplayName)
                    .NativeType("nvarchar(256)");

                m.Column(p => p.FirstName)
                    .NativeType("nvarchar(256)");

                m.Column(p => p.LastName)
                    .NativeType("nvarchar(256)");
            })
            .Execute(users);

        result.Should().Be(100);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        await using var session = Services.GetRequiredService<IDataSession>();
        var result = await session
            .MergeData("dbo.User")
            .Map<UserImport>(m => m
                .AutoMap()
                .Column(p => p.EmailAddress).Key()
            )
            .ExecuteAsync(users, cancellationToken: TestCancellation);

        result.Should().Be(100);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteOutputAsyncTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        await using var session = Services.GetRequiredService<IDataSession>();

        var changes = await session
            .MergeData("dbo.User")
            .Map<UserImport>(m => m
                .AutoMap()
                .Column(p => p.EmailAddress).Key()
            )
            .ExecuteOutputAsync(users, cancellationToken: TestCancellation);

        var result = changes.ToList();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncBulkCopyTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        await using var session = Services.GetRequiredService<IDataSession>();
        var result = await session
            .MergeData("dbo.User")
            .Mode(DataMergeMode.BulkCopy)
            .Map<UserImport>(m =>
            {
                m.Column(p => p.FirstName)
                    .NativeType("nvarchar(256)");

                m.Column(p => p.LastName)
                    .NativeType("nvarchar(256)");

                m.Column(p => p.EmailAddress)
                    .NativeType("nvarchar(256)")
                    .Key();

                m.Column(p => p.DisplayName)
                    .NativeType("nvarchar(256)");

            })
            .ExecuteAsync(users, cancellationToken: TestCancellation);

        result.Should().Be(100);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncBulkCopyAutoTest()
    {
        var generator = CreateGenerator();
        var users = generator.Generate(100);

        await using var session = Services.GetRequiredService<IDataSession>();
        var result = await session
            .MergeData("dbo.User")
            .Mode(DataMergeMode.BulkCopy)
            .Map<UserImport>(m =>
            {
                m.AutoMap();
                m.Column(p => p.EmailAddress).Key();
            })
            .ExecuteAsync(users, cancellationToken: TestCancellation);

        result.Should().Be(100);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncSqlStatementDateTimeTest()
    {
        await using var session = Services.GetRequiredService<IDataSession>();

        var item = CreateDataType();

        // dbo.DataType.DateTime is a legacy datetime column, which rejects strings with more than 3 fractional second digits
        var result = await session
            .MergeData<DataType>()
            .Mode(DataMergeMode.SqlStatement)
            .ExecuteAsync(new[] { item }, TestCancellation);

        result.Should().Be(1);

        await AssertDataType(session, item);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncSqlStatementDateFormatTest()
    {
        await using var session = Services.GetRequiredService<IDataSession>();

        // keep the connection open so the session level DATEFORMAT applies to the merge
        await session.EnsureConnectionAsync(TestCancellation);

        try
        {
            await session
                .Sql("SET DATEFORMAT dmy;")
                .ExecuteAsync(cancellationToken: TestCancellation);

            var item = CreateDataType();

            // a day greater than 12 fails when a yyyy-MM-dd string is read as ydm into a datetime column
            var result = await session
                .MergeData<DataType>()
                .Mode(DataMergeMode.SqlStatement)
                .ExecuteAsync(new[] { item }, TestCancellation);

            result.Should().Be(1);

            await AssertDataType(session, item);
        }
        finally
        {
            await session.ReleaseConnectionAsync();
        }
    }

    private static DataType CreateDataType()
    {
        var dateTime = new DateTime(2024, 5, 13, 8, 30, 15).AddTicks(1234567);

        return new DataType
        {
            Id = DateTime.Now.Ticks,
            Name = "Merge DateTime",
            Boolean = true,
            Short = 2,
            Long = 200,
            Float = 200.20F,
            Double = 300.35,
            Decimal = 456.12M,
            DateTime = dateTime,
            DateTimeOffset = new DateTimeOffset(dateTime, TimeSpan.FromHours(-6)),
            Guid = Guid.NewGuid(),
            TimeSpan = new TimeSpan(0, 1, 2, 3).Add(TimeSpan.FromTicks(1234567)),
            DateOnly = new DateOnly(2022, 12, 13),
            TimeOnly = new TimeOnly(1, 30, 0),
            DateTimeNull = dateTime,
            DateTimeOffsetNull = null,
            DateOnlyNull = new DateOnly(2022, 12, 13),
        };
    }

    private async System.Threading.Tasks.Task AssertDataType(IDataSession session, DataType expected)
    {
        var actual = await session
            .Sql(builder => builder
                .Select<DataType>()
                .Where(p => p.Id, expected.Id)
            )
            .QuerySingleAsync<DataType>(cancellationToken: TestCancellation);

        actual.Should().NotBeNull();

        // datetime is accurate to 1/300 of a second
        actual.DateTime.Should().BeCloseTo(expected.DateTime, TimeSpan.FromMilliseconds(4));
        actual.DateTimeNull.Should().BeCloseTo(expected.DateTimeNull!.Value, TimeSpan.FromMilliseconds(4));
        actual.DateTimeOffset.Should().BeCloseTo(expected.DateTimeOffset, TimeSpan.FromMicroseconds(1));
        actual.DateTimeOffsetNull.Should().BeNull();
        actual.TimeSpan.Should().Be(expected.TimeSpan);
        actual.DateOnly.Should().Be(expected.DateOnly);
        actual.DateOnlyNull.Should().Be(expected.DateOnlyNull);
        actual.TimeOnly.Should().Be(expected.TimeOnly);
    }

    private static Faker<UserImport> CreateGenerator()
    {
        var fakerUser = new Faker<UserImport>()
            .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
            .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
            .RuleFor(u => u.DisplayName, (f, u) => $"{u.FirstName} {u.LastName}")
            .RuleFor(u => u.EmailAddress, (f, u) => f.Internet.Email(u.FirstName, u.LastName, uniqueSuffix: $"+{DateTime.Now.Ticks}"));

        return fakerUser;
    }
}
