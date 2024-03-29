using System.Threading.Tasks;

using Azure.Storage.Blobs;

using FluentCommand.Caching;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Testcontainers.Redis;

using Xunit;

using XUnit.Hosting;

namespace FluentCommand.SqlServer.Tests;

public class DatabaseFixture : TestHostFixture, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("!P@ss0rd")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .Build();

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .Build();

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _azuriteContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await _azuriteContainer.DisposeAsync();
    }

    protected override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        base.ConfigureLogging(context, builder);
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // change database from container default
        var connectionBuilder = new SqlConnectionStringBuilder(_msSqlContainer.GetConnectionString())
        {
            InitialCatalog = "TrackerDocker"
        };

        var trackerConnection = connectionBuilder.ToString();
        var cacheConnection = _redisContainer.GetConnectionString();
        var azuriteConnection = _azuriteContainer.GetConnectionString();

        services.AddHostedService<DatabaseInitializer>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheConnection;
            options.InstanceName = "FluentCommand";
        });

        services.AddFluentCommand(builder => builder
            .UseConnectionString(trackerConnection)
            .UseSqlServer()
            .AddDistributedDataCache()
        );

        // azurite tesing
        services.AddSingleton(sp => new BlobContainerClient(azuriteConnection, "fluent-command-testing"));

        // readonly intent connection
        connectionBuilder.ApplicationIntent = ApplicationIntent.ReadOnly;

        var readOnlyConnection = connectionBuilder.ToString();

        services.AddFluentCommand<ReadOnlyIntent>(builder => builder
            .UseConnectionString(readOnlyConnection)
            .UseSqlServer()
            .AddDistributedDataCache()
        );
    }
}
