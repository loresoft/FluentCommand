using FluentCommand.Entities;
using FluentCommand.Query;

using Microsoft.Extensions.DependencyInjection;

using XUnit.Hosting.Logging;

namespace FluentCommand.SqlServer.Tests;

public class DataCacheTests : DatabaseTestBase
{
    public DataCacheTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async System.Threading.Tasks.Task SqlQueryInEntityAsync()
    {
        await using var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var values = new[] { 1, 2, 3 };

        var results = await session
            .Sql(builder => builder
                .Select<Status>()
                .WhereIn(p => p.Id, values)
            )
            .UseCache(TimeSpan.FromSeconds(5))
            .QueryAsync<Status>();

        results.Should().NotBeNull();

        var list = results.ToList();
        list.Count.Should().Be(3);

        var cacheResults = await session
            .Sql(builder => builder
                .Select<Status>()
                .WhereIn(p => p.Id, values)
            )
            .UseCache(TimeSpan.FromSeconds(5))
            .QueryAsync<Status>();

        // check logs for cache hit
        var memoryLoggerProvider = Services.GetRequiredService<MemoryLoggerProvider>();

        var logs = memoryLoggerProvider.Logs();
        var hasHit = logs.Any(l => l.Message.Contains("Cache Hit;"));

        hasHit.Should().BeTrue();
    }

}
