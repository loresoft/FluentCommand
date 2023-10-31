using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using Testcontainers.PostgreSql;

using Xunit;

using XUnit.Hosting;

namespace FluentCommand.PostgreSQL.Tests;

public class DatabaseFixture : TestHostFixture, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("TrackerDocker")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }


    protected override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        base.ConfigureLogging(context, builder);
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var trackerConnection = _postgreSqlContainer.GetConnectionString();

        services.AddHostedService<DatabaseInitializer>();

        services.AddFluentCommand(builder => builder
            .UseConnectionString(trackerConnection)
            .AddProviderFactory(NpgsqlFactory.Instance)
            .AddPostgreSqlGenerator()
        );
    }
}
