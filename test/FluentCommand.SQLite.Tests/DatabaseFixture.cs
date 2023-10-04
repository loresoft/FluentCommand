using FluentCommand.Caching;
using FluentCommand.Query.Generators;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        var trackerConnection = context.Configuration.GetConnectionString("Tracker");
        var cacheConnection = context.Configuration.GetConnectionString("DistributedCache");

        services.AddHostedService<DatabaseInitializer>();
                
        services.TryAddSingleton<IQueryGenerator, SqliteGenerator>();
        services.TryAddSingleton<IDataQueryLogger, DataQueryLogger>();

        services.TryAddSingleton<IDataConfiguration>(sp =>
            new DataConfiguration(
                SqliteFactory.Instance,
                trackerConnection,
                sp.GetService<IDataCache>(),
                sp.GetService<IQueryGenerator>(),
                sp.GetService<IDataQueryLogger>()
            )
        );

        services.TryAddTransient<IDataSession>(sp =>
            new DataSession(sp.GetRequiredService<IDataConfiguration>())
        );
    }
}
