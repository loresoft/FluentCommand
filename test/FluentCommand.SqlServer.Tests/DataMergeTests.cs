using Bogus;

using FluentCommand.Entities;
using FluentCommand.Merge;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataMergeTests : DatabaseTestBase
{
    public DataMergeTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
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
            .ExecuteAsync(users);

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
            .ExecuteOutputAsync(users);

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
            .ExecuteAsync(users);

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
            .ExecuteAsync(users);

        result.Should().Be(100);
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
