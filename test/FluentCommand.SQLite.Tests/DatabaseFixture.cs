using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using XUnit.Hosting;

namespace FluentCommand.SQLite.Tests;

public class DatabaseFixture : TestApplicationFixture
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        var services = builder.Services;

        services.AddHostedService<DatabaseInitializer>();

        services.AddFluentCommand(builder => builder
            .UseConnectionName("Tracker")
            .AddProviderFactory(SqliteFactory.Instance)
            .AddSqliteGenerator()
        );
    }
}
