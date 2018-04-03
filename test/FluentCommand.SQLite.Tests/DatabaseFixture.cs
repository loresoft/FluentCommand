using System;
using System.Data.SQLite;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace FluentCommand.SQLite.Tests
{
    public class DatabaseFixture : IDisposable
    {
        private readonly StringWriter _logger;

        public DatabaseFixture()
        {
            _logger = new StringWriter();

            ResolveConnectionString();

            CreateDatabase();
        }


        public string ConnectionString { get; set; }


        private void CreateDatabase()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                using (var schemaCommand = connection.CreateCommand())
                {
                    schemaCommand.CommandType = System.Data.CommandType.Text;
                    schemaCommand.CommandText = Data.SqlStatements.TrackerSchema;
                    var schemaResult = schemaCommand.ExecuteNonQuery();

                    _logger.WriteLine($"Created Schema: '{schemaResult}'");
                }

                using (var dataCommand = connection.CreateCommand())
                {
                    dataCommand.CommandType = System.Data.CommandType.Text;
                    dataCommand.CommandText = Data.SqlStatements.TrackerData;
                    var dataResult = dataCommand.ExecuteNonQuery();

                    _logger.WriteLine($"Created Data: '{dataResult}'");
                }
            }
        }

        private void ResolveConnectionString()
        {
            //"Data Source=Tracker.db;Version=3;";
            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = $"Tracker-{Guid.NewGuid():N}.db";
            builder.Version = 3;

            _logger.WriteLine($"ConnectionString: '{builder}'");

            ConnectionString = builder.ToString();
        }


        public void Report(ITestOutputHelper output)
        {
            output.WriteLine(_logger.ToString());

            // reset logger
            _logger.GetStringBuilder().Clear();
        }

        public void Dispose()
        {

        }
    }
}