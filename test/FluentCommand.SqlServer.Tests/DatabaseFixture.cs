using FluentCommand.Caching;
using FluentCommand.Query.Generators;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using XUnit.Hosting;

namespace FluentCommand.SqlServer.Tests;

public class DatabaseFixture : TestHostFixture
{
    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var trackerConnnection = context.Configuration.GetConnectionString("Tracker");
        var cacheConnection = context.Configuration.GetConnectionString("DistributedCache");

        services.AddHostedService<DatabaseInitializer>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheConnection;
            options.InstanceName = "FluentCommand";
        });

        services.TryAddSingleton<IDistributedCacheSerializer>(sp => new MessagePackCacheSerializer());

        services.TryAddSingleton<IDataCache, DistributedDataCache>();
        services.TryAddSingleton<IQueryGenerator, SqlServerGenerator>();
        services.TryAddSingleton<IDataQueryLogger, DatabaseQueryLogger>();

        services.TryAddSingleton<IDataConfiguration>(sp =>
            new DataConfiguration(
                SqlClientFactory.Instance,
                trackerConnnection,
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
