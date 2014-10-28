using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using FluentAssertions;
using Xunit;

namespace FluentCommand.SqlServer.Tests
{
    
    public class DataSessionTests
    {
        
        [Fact]
        public void CreateConnectionName()
        {
            var session = new DataSession("Tracker");
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
        public void CreateMissingConnectonName()
        {
            Action create = () => new DataSession("Blah");
            create.ShouldThrow<ConfigurationErrorsException>();
        }

        
        [Fact]
        public void CreateMissingPrividerName()
        {
            Action create = () => new DataSession("TrackerNoProvider");
            create.ShouldThrow<ConfigurationErrorsException>();
        }

        
        [Fact]
        public void EnsureConnectionByName()
        {
            var session = new DataSession("Tracker");
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
