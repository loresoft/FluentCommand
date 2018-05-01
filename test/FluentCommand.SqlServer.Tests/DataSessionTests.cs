using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests
{
    public class DataSessionTests : DatabaseTestBase
    {
        public DataSessionTests(ITestOutputHelper output, DatabaseFixture databaseFixture) : base(output, databaseFixture)
        {
        }

        [Fact]
        public void CreateConnectionName()
        {
            var session = GetConfiguration().CreateSession();
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
            var session = GetConfiguration().CreateSession();
            session.Should().NotBeNull();
            session.Connection.Should().NotBeNull();
            session.Connection.State.Should().Be(ConnectionState.Closed);

            session.EnsureConnection();
            session.Connection.State.Should().Be(ConnectionState.Open);

            session.ReleaseConnection();
            session.Connection.State.Should().Be(ConnectionState.Closed);
        }
    }
}
