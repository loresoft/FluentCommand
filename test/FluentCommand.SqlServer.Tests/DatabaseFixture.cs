using Azure.Storage.Blobs;

using FluentCommand.Caching;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Testcontainers.Azurite;
using Testcontainers.MsSql;
using Testcontainers.Redis;

using XUnit.Hosting;

using static Azure.Storage.Blobs.BlobClientOptions;

namespace FluentCommand.SqlServer.Tests;

public class DatabaseFixture : TestApplicationFixture, IAsyncLifetime
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

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        var services = builder.Services;

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
        services.AddSingleton(sp => new BlobContainerClient(azuriteConnection, "fluent-command-testing", new BlobClientOptions(ServiceVersion.V2023_11_03)));

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
