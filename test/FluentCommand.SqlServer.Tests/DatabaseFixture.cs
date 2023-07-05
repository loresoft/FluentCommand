using System;
using System.IO;
using System.Reflection;
using System.Text;

using DbUp;
using DbUp.Engine.Output;

using Microsoft.Extensions.Configuration;

using Xunit.Abstractions;

namespace FluentCommand.SqlServer.Tests;

public class DatabaseFixture : IUpgradeLog, IDisposable
{
    private readonly StringBuilder _buffer;
    private readonly StringWriter _logger;

    public DatabaseFixture()
    {
        _buffer = new StringBuilder();
        _logger = new StringWriter(_buffer);

        ResolveConnectionString();

        CreateDatabase();
    }


    public string ConnectionString { get; set; }

    public string ConnectionName { get; set; } = "Tracker";


    private void CreateDatabase()
    {
        EnsureDatabase.For
            .SqlDatabase(ConnectionString, this);

        var upgradeEngine = DeployChanges.To
                .SqlDatabase(ConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogTo(this)
                .Build();

        var result = upgradeEngine.PerformUpgrade();

        if (result.Successful)
            return;

        _logger.WriteLine($"Exception: '{result.Error}'");

        throw result.Error;
    }

    private void ResolveConnectionString()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Test";
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        ConnectionString = configuration.GetConnectionString(ConnectionName);
    }


    public void Report(ITestOutputHelper output)
    {
        if (_buffer.Length == 0)
            return;

        _logger.Flush();
        output.WriteLine(_logger.ToString());

        // reset logger
        _buffer.Clear();
    }

    public void Dispose()
    {

    }


    public void WriteInformation(string format, params object[] args)
    {
        _logger.Write("INFO : ");
        _logger.WriteLine(format, args);
    }

    public void WriteError(string format, params object[] args)
    {
        _logger.Write("ERROR: ");
        _logger.WriteLine(format, args);
    }

    public void WriteWarning(string format, params object[] args)
    {
        _logger.Write("WARN : ");
        _logger.WriteLine(format, args);
    }

}
