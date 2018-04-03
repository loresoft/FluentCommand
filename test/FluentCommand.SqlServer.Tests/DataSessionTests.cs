using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests
{
    public class DataSessionTests : DataTestBase
    {
        public DataSessionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void CreateConnectionName()
        {
            var session = GetConfiguration("Tracker").CreateSession();
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
        public void CreateMissingConnectionName()
        {
            Action create = () => GetConfiguration("Blah").CreateSession();
            create.Should().Throw<InvalidOperationException>();
        }
        
        [Fact]
        public void EnsureConnectionByName()
        {
            var session = GetConfiguration("Tracker").CreateSession();
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
