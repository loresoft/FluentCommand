using FluentCommand.Entities;
using FluentCommand.Query;

using Microsoft.Extensions.DependencyInjection;

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
        var session = Services.GetRequiredService<IDataSession>();
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
        var session = Services.GetRequiredService<IDataSession>();
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
        var session = Services.GetRequiredService<IDataSession>();
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
        var session = Services.GetRequiredService<IDataSession>();
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
