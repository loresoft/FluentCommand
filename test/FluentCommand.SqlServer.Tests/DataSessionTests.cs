using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.SqlServer.Tests;

public class DataSessionTests : DatabaseTestBase
{
    public DataSessionTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public void CreateConnectionName()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();
        session.Connection.Should().NotBeNull();
        session.Connection.State.Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public void CreateConnection()
    {
        var sqlConnection = new SqlConnection("Data Source=(local);Initial Catalog=Tracker;Integrated Security=True;");
        var session = new DataSession(sqlConnection);
        session.Should().NotBeNull();
        session.Connection.Should().NotBeNull();
        session.Connection.State.Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public void EnsureConnectionByName()
    {
        var session = Services.GetRequiredService<IDataSession>();
        session.Should().NotBeNull();
        session.Connection.Should().NotBeNull();
        session.Connection.State.Should().Be(ConnectionState.Closed);

        session.EnsureConnection();
        session.Connection.State.Should().Be(ConnectionState.Open);

        session.ReleaseConnection();
        session.Connection.State.Should().Be(ConnectionState.Closed);
    }
}
