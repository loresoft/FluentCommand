using FluentAssertions;

using FluentCommand.Entities;
using FluentCommand.Query;

using Xunit;
using Xunit.Abstractions;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SqlServer.Tests;

public class GeneratorTests : DatabaseTestBase
{
    public GeneratorTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task QuerySelectStatusAsync()
    {
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        var results = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryAsync<Status>();

        results.Should().NotBeNull();
    }

    [Fact]
    public async Task QuerySelectStatusRecordAsync()
    {
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        var results = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryAsync<StatusRecord>();

        results.Should().NotBeNull();
    }

    [Fact]
    public async Task QuerySelectStatusReadOnlyAsync()
    {
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        var results = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryAsync<StatusReadOnly>();

        results.Should().NotBeNull();
    }

    [Fact]
    public async Task QuerySelectStatusConstructorAsync()
    {
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        var results = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryAsync<StatusConstructor>();

        results.Should().NotBeNull();
    }
}
