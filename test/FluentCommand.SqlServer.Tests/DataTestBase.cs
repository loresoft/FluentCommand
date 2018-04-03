using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests
{
    public abstract class DataTestBase
    {
        private ITestOutputHelper _output;

        protected DataTestBase(ITestOutputHelper output)
        {
            _output = output;
        }

        protected IDataConfiguration GetConfiguration(string connectionName)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Test";
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var connectionString = configuration.GetConnectionString(connectionName);

            var dataConfiguration  = new DataConfiguration(SqlClientFactory.Instance, connectionString, null, _output.WriteLine);

            return dataConfiguration;
        }
    }
}