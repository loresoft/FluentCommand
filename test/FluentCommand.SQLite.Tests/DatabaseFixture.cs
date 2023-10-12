using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using XUnit.Hosting;

namespace FluentCommand.SQLite.Tests;

public class DatabaseFixture : TestHostFixture
{
    protected override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        base.ConfigureLogging(context, builder);
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddHostedService<DatabaseInitializer>();

        services.AddFluentCommand(builder => builder
            .UseConnectionName("Tracker")
            .AddProviderFactory(SqliteFactory.Instance)
            .AddSqliteGenerator()
        );
    }
}
