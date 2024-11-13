using System.Reflection;
using System.Text;

using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentCommand.SqlServer.Tests;

public class DatabaseInitializer : IHostedService, IUpgradeLog
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IDataConfiguration _configuration;

    public DatabaseInitializer(ILogger<DatabaseInitializer> logger, IDataConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.ConnectionString;

        // create database
        EnsureDatabase.For
            .SqlDatabase(connectionString, this);

        // parse connection string
        var builder = new SqlConnectionStringBuilder(connectionString);
        var database = builder.InitialCatalog;

        var upgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScript(
                    "enable-change-tracking",
                    $"ALTER DATABASE [{database}] SET CHANGE_TRACKING = ON ( CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON );",
                    new SqlScriptOptions { RunGroupOrder = 0 }
                )
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    Encoding.Default,
                    new SqlScriptOptions { RunGroupOrder = 1 }
                )
                .LogTo(this)
                .Build();

        var result = upgradeEngine.PerformUpgrade();

        return result.Successful
            ? Task.CompletedTask
            : Task.FromException(result.Error);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    public void LogDebug(string format, params object[] args)
    {
        _logger.LogDebug(format, args);
    }

    public void LogError(string format, params object[] args)
    {
        _logger.LogError(format, args);
    }

    public void LogError(Exception ex, string format, params object[] args)
    {
        _logger.LogError(ex, format, args);
    }

    public void LogInformation(string format, params object[] args)
    {
        _logger.LogInformation(format, args);
    }

    public void LogTrace(string format, params object[] args)
    {
        _logger.LogTrace(format, args);
    }

    public void LogWarning(string format, params object[] args)
    {
        _logger.LogWarning(format, args);
    }
}
