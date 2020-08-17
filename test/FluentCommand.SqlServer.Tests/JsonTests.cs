using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests
{
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
    }
}