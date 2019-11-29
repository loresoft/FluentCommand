using System;
using DataGenerator;
using DataGenerator.Sources;
using FluentAssertions;
using FluentCommand.Entities;
using FluentCommand.Merge;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests
{
    public class DataMergeTests : DatabaseTestBase
    {
        public DataMergeTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
            : base(output, databaseFixture)
        {
        }

        [Fact]
        public void MergeDataTest()
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
                    .Merge(users);
            }

            result.Should().Be(100);
        }
    }
}