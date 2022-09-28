using FluentAssertions;

using FluentCommand.Entities;

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
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = session.Sql(sql)
            .QueryJson();

        json.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryJsonAsync()
    {
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = await session.Sql(sql)
            .QueryJsonAsync();

        json.Should().NotBeNull();
    }


    [Fact]
    public async Task QuerySelectAsync()
    {
        var session = GetConfiguration().CreateSession();
        session.Should().NotBeNull();

        string sql = "select TOP 1000 * from [User]";

        var json = await session
            .Sql(builder => builder
                .Select<Status>()
                .Column("Id")
                .Column(p => p.Name)
                .Column("Description")
                .Where(p => p.IsActive, true)
                .Where(b => b
                    .Or(o => o
                        .Where("Name", "Test", FilterOperators.Contains)
                        .Where(p => p.Description, "Test", FilterOperators.Contains)
                    )
                )
                .OrderBy(p => p.DisplayOrder, SortDirections.Descending)
                .OrderBy("Name")
            )
            .QueryJsonAsync();

        json.Should().NotBeNull();
    }
}
