using System.Reflection;

using DbUp;
using DbUp.Engine.Output;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentCommand.PostgreSQL.Tests;

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

        EnsureDatabase.For.PostgresqlDatabase(connectionString, this);

        var upgradeEngine = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
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


    public void WriteError(string format, params object[] args)
    {
        _logger.LogError(format, args);
    }

    public void WriteInformation(string format, params object[] args)
    {
        _logger.LogInformation(format, args);
    }

    public void WriteWarning(string format, params object[] args)
    {
        _logger.LogWarning(format, args);
    }
}
