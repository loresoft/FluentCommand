using System;
using System.Collections.Generic;
using System.Linq;

using DataGenerator;
using DataGenerator.Sources;

using FluentAssertions;

using FluentCommand.Entities;
using FluentCommand.Merge;

using Xunit;
using Xunit.Abstractions;

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
        var generator = UserImportGenerator();
        var users = generator.List<UserImport>(100);

        int result;
        using (var session = GetConfiguration().CreateSession())
        {
            result = session
                .MergeData("dbo.User")
                .Map<UserImport>(m => m
                    .AutoMap()
                    .Column(p => p.EmailAddress).Key()
                )
                .Execute(users);
        }

        result.Should().Be(100);
    }

    [Fact]
    public void ExecuteOutputTest()
    {
        var generator = UserImportGenerator();
        var users = generator.List<UserImport>(100);

        List<DataMergeOutputRow> result;
        using (var session = GetConfiguration().CreateSession())
        {
            result = session
                .MergeData("dbo.User")
                .Map<UserImport>(m => m
                    .AutoMap()
                    .Column(p => p.EmailAddress).Key()
                )
                .ExecuteOutput(users)
                .ToList();
        }

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ExecuteBulkCopyTest()
    {
        var generator = UserImportGenerator();
        var users = generator.List<UserImport>(100);

        int result;
        using (var session = GetConfiguration().CreateSession())
        {
            result = session
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
        }

        result.Should().Be(100);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncTest()
    {
        var generator = UserImportGenerator();
        var users = generator.List<UserImport>(100);

        int result;
        using (var session = GetConfiguration().CreateSession())
        {
            result = await session
                .MergeData("dbo.User")
                .Map<UserImport>(m => m
                    .AutoMap()
                    .Column(p => p.EmailAddress).Key()
                )
                .ExecuteAsync(users);
        }

        result.Should().Be(100);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteOutputAsyncTest()
    {
        var generator = UserImportGenerator();
        var users = generator.List<UserImport>(100);

        List<DataMergeOutputRow> result;
        using (var session = GetConfiguration().CreateSession())
        {
            var changes = await session
                .MergeData("dbo.User")
                .Map<UserImport>(m => m
                    .AutoMap()
                    .Column(p => p.EmailAddress).Key()
                )
                .ExecuteOutputAsync(users);

            result = changes.ToList();
        }

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsyncBulkCopyTest()
    {
        var generator = UserImportGenerator();
        var users = generator.List<UserImport>(100);

        int result;
        using (var session = GetConfiguration().CreateSession())
        {
            result = await session
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
                .ExecuteAsync(users);
        }

        result.Should().Be(100);
    }

    private static Generator UserImportGenerator()
    {
        var generator = Generator.Create(c => c
            .ExcludeName("xunit")
            .Entity<UserImport>(e =>
            {
                e.AutoMap();

                e.Property(p => p.DisplayName).DataSource<NameSource>();
                e.Property(p => p.EmailAddress).Value(u => $"MergeTest.{Guid.NewGuid()}@mailinator.com");
            })
        );

        return generator;
    }
}
