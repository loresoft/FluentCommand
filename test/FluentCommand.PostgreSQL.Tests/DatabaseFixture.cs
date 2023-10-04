using FluentCommand.Query.Generators;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

using XUnit.Hosting;

namespace FluentCommand.PostgreSQL.Tests;

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

        services.TryAddSingleton<IQueryGenerator, PostgreSqlGenerator>();
        services.TryAddSingleton<IDataQueryLogger, DataQueryLogger>();

        services.TryAddSingleton<IDataConfiguration>(sp =>
            new DataConfiguration(
                NpgsqlFactory.Instance,
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
