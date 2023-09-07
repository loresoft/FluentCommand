using FluentAssertions;

using FluentCommand.Entities;
using FluentCommand.Query;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

using Task = System.Threading.Tasks.Task;

namespace FluentCommand.SqlServer.Tests;

public class JsonTests : DatabaseTestBase
{
    public JsonTests(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {

    }

    [Fact]
    public void QueryJson()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = session.Sql(sql)
            .QueryJson();

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryJsonAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = await session.Sql(sql)
            .QueryJsonAsync();

        json.Should().NotBeNull();
    }


    [Fact]
    public async Task QuerySelectAsync()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();

        var json = await session
            .Sql(builder => builder
                .Select<Status>()
                .OrderBy(p => p.DisplayOrder)
                .Limit(0, 1000)
            )
            .QueryJsonAsync();

        json.Should().NotBeNull();
    }
}
