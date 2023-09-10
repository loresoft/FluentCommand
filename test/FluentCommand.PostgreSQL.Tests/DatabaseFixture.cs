using FluentCommand.Query.Generators;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Npgsql;

using XUnit.Hosting;

namespace FluentCommand.PostgreSQL.Tests;

public class DatabaseFixture : TestHostFixture
{
    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        var trackerConnnection = context.Configuration.GetConnectionString("Tracker");
        var cacheConnection = context.Configuration.GetConnectionString("DistributedCache");

        services.AddHostedService<DatabaseInitializer>();

        services.TryAddSingleton<IQueryGenerator, PostgresqlGenerator>();
        services.TryAddSingleton<IDataQueryLogger, DatabaseQueryLogger>();

        services.TryAddSingleton<IDataConfiguration>(sp =>
            new DataConfiguration(
                NpgsqlFactory.Instance,
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
