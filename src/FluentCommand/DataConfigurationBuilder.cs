using System.Data.Common;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand;

public class DataConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private string _connectionName;
    private string _connectionString;
    private Type _providerFactoryType;
    private Type _dataCacheType;
    private Type _queryGeneratorType;
    private Type _queryLoggerType;

    public DataConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
    }


    public DataConfigurationBuilder UseConnectionName(string connectionName)
    {
        _connectionName = connectionName;
        return this;
    }

    public DataConfigurationBuilder UseConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }


    public DataConfigurationBuilder AddProviderFactory<TService>(TService providerFactory)
        where TService : DbProviderFactory
    {
        _providerFactoryType = typeof(TService);
        _services.TryAddSingleton(providerFactory);
        return this;
    }

    public DataConfigurationBuilder AddProviderFactory<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : DbProviderFactory
    {
        _providerFactoryType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    public DataConfigurationBuilder AddProviderFactory<TService>()
        where TService : DbProviderFactory
    {
        _providerFactoryType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }


    public DataConfigurationBuilder AddDataCache<TService>(TService dataCache)
        where TService : class, IDataCache
    {
        _dataCacheType = typeof(TService);
        _services.TryAddSingleton(dataCache);
        return this;
    }

    public DataConfigurationBuilder AddDataCache<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class, IDataCache
    {
        _dataCacheType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    public DataConfigurationBuilder AddDataCache<TService>()
        where TService : class, IDataCache
    {
        _dataCacheType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }


    public DataConfigurationBuilder AddQueryGenerator<TService>(TService queryGenerator)
        where TService : class, IQueryGenerator
    {
        _queryGeneratorType = typeof(TService);
        _services.TryAddSingleton(queryGenerator);
        return this;
    }

    public DataConfigurationBuilder AddQueryGenerator<TService>()
        where TService : class, IQueryGenerator
    {
        _queryGeneratorType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }

    public DataConfigurationBuilder AddQueryGenerator<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class, IQueryGenerator
    {
        _queryGeneratorType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    public DataConfigurationBuilder AddSqlServerGenerator()
    {
        AddQueryGenerator<SqlServerGenerator>();
        return this;
    }

    public DataConfigurationBuilder AddSqliteGenerator()
    {
        AddQueryGenerator<SqliteGenerator>();
        return this;
    }

    public DataConfigurationBuilder AddPostgreSqlGenerator()
    {
        AddQueryGenerator<PostgreSqlGenerator>();
        return this;
    }


    public DataConfigurationBuilder AddQueryLogger<TService>(TService queryLogger)
        where TService : class, IDataQueryLogger
    {
        _queryLoggerType = typeof(TService);
        _services.TryAddSingleton(queryLogger);
        return this;
    }

    public DataConfigurationBuilder AddQueryLogger<TService>()
        where TService : class, IDataQueryLogger
    {
        _queryLoggerType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }

    public DataConfigurationBuilder AddQueryLogger<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class, IDataQueryLogger
    {
        _queryLoggerType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }


    public DataConfigurationBuilder AddService(Action<IServiceCollection> setupAction)
    {
        setupAction(_services);
        return this;
    }


    internal void AddConfiguration()
    {
        RegisterDefaults();

        // resolve using specific types to support multiple configurations
        var providerFactory = _providerFactoryType ?? typeof(DbProviderFactory);
        var dataCache = _dataCacheType ?? typeof(IDataCache);
        var queryGenerator = _queryGeneratorType ?? typeof(IQueryGenerator);
        var queryLogger = _queryLoggerType ?? typeof(IDataQueryLogger);

        if (_connectionName.HasValue())
        {
            _services.TryAddSingleton<IDataConfiguration>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(_connectionName);

                return new DataConfiguration(
                    sp.GetRequiredService(providerFactory) as DbProviderFactory,
                    connectionString,
                    sp.GetService(dataCache) as IDataCache,
                    sp.GetService(queryGenerator) as IQueryGenerator,
                    sp.GetService(queryLogger) as IDataQueryLogger
                );
            });
        }
        else
        {
            _services.TryAddSingleton<IDataConfiguration>(sp =>
                new DataConfiguration(
                    sp.GetRequiredService(providerFactory) as DbProviderFactory,
                    _connectionString,
                    sp.GetService(dataCache) as IDataCache,
                    sp.GetService(queryGenerator) as IQueryGenerator,
                    sp.GetService(queryLogger) as IDataQueryLogger
                )
            );
        }

        _services.TryAddTransient<IDataSessionFactory>(sp => sp.GetService<IDataConfiguration>());
        _services.TryAddTransient<IDataSession, DataSession>();
    }

    internal void AddConfiguration<TDiscriminator>()
    {
        RegisterDefaults();

        // resolve using specific types to support multiple configurations
        var providerFactory = _providerFactoryType ?? typeof(DbProviderFactory);
        var dataCache = _dataCacheType ?? typeof(IDataCache);
        var queryGenerator = _queryGeneratorType ?? typeof(IQueryGenerator);
        var queryLogger = _queryLoggerType ?? typeof(IDataQueryLogger);

        if (_connectionName.HasValue())
        {
            _services.TryAddSingleton<IDataConfiguration<TDiscriminator>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(_connectionName);

                return new DataConfiguration<TDiscriminator>(
                    sp.GetRequiredService(providerFactory) as DbProviderFactory,
                    connectionString,
                    sp.GetService(dataCache) as IDataCache,
                    sp.GetService(queryGenerator) as IQueryGenerator,
                    sp.GetService(queryLogger) as IDataQueryLogger
                );
            });
        }
        else
        {
            _services.TryAddSingleton<IDataConfiguration<TDiscriminator>>(sp =>
                new DataConfiguration<TDiscriminator>(
                    sp.GetRequiredService(providerFactory) as DbProviderFactory,
                    _connectionString,
                    sp.GetService(dataCache) as IDataCache,
                    sp.GetService(queryGenerator) as IQueryGenerator,
                    sp.GetService(queryLogger) as IDataQueryLogger
                )
            );
        }

        _services.TryAddTransient<IDataSessionFactory<TDiscriminator>>(sp => sp.GetService<IDataConfiguration<TDiscriminator>>());
        _services.TryAddTransient<IDataSession<TDiscriminator>, DataSession<TDiscriminator>>();
    }

    private void RegisterDefaults()
    {
        // add defaults if not already added
        _services.TryAddSingleton<IDataQueryFormatter, DataQueryFormatter>();

        // convert specific types to interfaces
        if (_providerFactoryType != null)
            _services.TryAddSingleton(sp => sp.GetService(_providerFactoryType) as DbProviderFactory);

        if (_dataCacheType != null)
            _services.TryAddSingleton(sp => sp.GetService(_dataCacheType) as IDataCache);

        if (_queryGeneratorType != null)
            _services.TryAddSingleton(sp => sp.GetService(_queryGeneratorType) as IQueryGenerator);
        else
            _services.TryAddSingleton<IQueryGenerator, SqlServerGenerator>();

        if (_queryLoggerType != null)
            _services.TryAddSingleton(sp => sp.GetService(_queryLoggerType) as IDataQueryLogger);
        else
            _services.TryAddSingleton<IDataQueryLogger, DataQueryLogger>();

    }
}
