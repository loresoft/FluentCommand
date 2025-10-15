using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql;

using Testcontainers.PostgreSql;

using XUnit.Hosting;

namespace FluentCommand.PostgreSQL.Tests;

public class DatabaseFixture : TestApplicationFixture, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("TrackerDocker")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        var services = builder.Services;
        var trackerConnection = _postgreSqlContainer.GetConnectionString();

        services.AddHostedService<DatabaseInitializer>();

        services.AddFluentCommand(builder => builder
            .UseConnectionString(trackerConnection)
            .AddProviderFactory(NpgsqlFactory.Instance)
            .AddPostgreSqlGenerator()
        );
    }
}
