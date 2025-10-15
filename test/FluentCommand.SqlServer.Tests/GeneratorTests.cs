using FluentCommand.Entities;
using FluentCommand.Query;

using Microsoft.Extensions.DependencyInjection;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SqlServer.Tests;

public class GeneratorTests : DatabaseTestBase
{
    public GeneratorTests(DatabaseFixture databaseFixture)
        : base(databaseFixture)
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
            .QueryAsync<Status>(cancellationToken: TestCancellation);

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
            .QueryAsync<StatusRecord>(cancellationToken: TestCancellation);

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
            .QueryAsync<StatusReadOnly>(cancellationToken: TestCancellation);

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
            .QueryAsync<StatusConstructor>(cancellationToken: TestCancellation);

        results.Should().NotBeNull();
    }
}
