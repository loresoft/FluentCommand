using System.Data.Common;

using FluentCommand.Query.Generators;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand;

public class DataConfigurationBuilder
{
    private readonly IServiceCollection _services;

    public DataConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
    }


    public DataConfigurationBuilder UseConnectionName(string connectionName)
    {
        _services.TryAddSingleton<IDataConfiguration>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(connectionName);

            return new DataConfiguration(
                sp.GetRequiredService<DbProviderFactory>(),
                connectionString,
                sp.GetService<IDataCache>(),
                sp.GetService<IQueryGenerator>(),
                sp.GetService<IDataQueryLogger>()
            );
        });

        return this;
    }

    public DataConfigurationBuilder UseConnectionString(string connectionString)
    {
        _services.TryAddSingleton<IDataConfiguration>(sp =>
            new DataConfiguration(
                sp.GetRequiredService<DbProviderFactory>(),
                connectionString,
                sp.GetService<IDataCache>(),
                sp.GetService<IQueryGenerator>(),
                sp.GetService<IDataQueryLogger>()
            )
        );

        return this;
    }


    public DataConfigurationBuilder AddProviderFactory(DbProviderFactory providerFactory)
    {
        _services.TryAddSingleton(providerFactory);
        return this;
    }

    public DataConfigurationBuilder AddProviderFactory(Func<IServiceProvider, DbProviderFactory> implementationFactory)
    {
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    public DataConfigurationBuilder AddProviderFactory<TService>()
        where TService : DbProviderFactory
    {
        _services.TryAddSingleton<DbProviderFactory, TService>();
        return this;
    }


    public DataConfigurationBuilder AddDataCache(IDataCache dataCache)
    {
        _services.TryAddSingleton(dataCache);
        return this;
    }

    public DataConfigurationBuilder AddDataCache(Func<IServiceProvider, IDataCache> implementationFactory)
    {
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    public DataConfigurationBuilder AddDataCache<TService>()
        where TService : class, IDataCache
    {
        _services.TryAddSingleton<IDataCache, TService>();
        return this;
    }


    public DataConfigurationBuilder AddQueryGenerator(IQueryGenerator queryGenerator)
    {
        _services.TryAddSingleton(queryGenerator);
        return this;
    }

    public DataConfigurationBuilder AddQueryGenerator<TService>()
        where TService : class, IQueryGenerator
    {
        _services.TryAddSingleton<IQueryGenerator, TService>();
        return this;
    }

    public DataConfigurationBuilder AddQueryGenerator(Func<IServiceProvider, IQueryGenerator> implementationFactory)
    {
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    public DataConfigurationBuilder AddSqlServerGenerator()
    {
        _services.TryAddSingleton<IQueryGenerator, SqlServerGenerator>();
        return this;
    }

    public DataConfigurationBuilder AddSqliteGenerator()
    {
        _services.TryAddSingleton<IQueryGenerator, SqliteGenerator>();
        return this;
    }

    public DataConfigurationBuilder AddPostgreSqlGenerator()
    {
        _services.TryAddSingleton<IQueryGenerator, PostgreSqlGenerator>();
        return this;
    }


    public DataConfigurationBuilder AddQueryLogger(IDataQueryLogger queryLogger)
    {
        _services.TryAddSingleton(queryLogger);
        return this;
    }

    public DataConfigurationBuilder AddQueryLogger<TService>()
        where TService : class, IDataQueryLogger
    {
        _services.TryAddSingleton<IDataQueryLogger, TService>();
        return this;
    }

    public DataConfigurationBuilder AddQueryLogger(Func<IServiceProvider, IDataQueryLogger> implementationFactory)
    {
        _services.TryAddSingleton(implementationFactory);
        return this;
    }
}
