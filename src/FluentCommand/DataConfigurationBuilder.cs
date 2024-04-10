using System.Data.Common;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentCommand;

/// <summary>
/// A configuration builder class
/// </summary>
public class DataConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private string _nameOrConnectionString;
    private Type _providerFactoryType;
    private Type _dataCacheType;
    private Type _queryGeneratorType;
    private Type _queryLoggerType;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataConfigurationBuilder"/> class.
    /// </summary>
    /// <param name="services">The services.</param>
    public DataConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
    }


    /// <summary>
    /// Set the connection string or the name of connection string located in the application configuration
    /// </summary>
    /// <param name="nameOrConnectionString">The connection string or the name of connection string located in the application configuration.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    public DataConfigurationBuilder UseConnectionName(string nameOrConnectionString)
    {
        _nameOrConnectionString = nameOrConnectionString;
        return this;
    }

    /// <summary>
    /// The connection string to use with fluent command.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    public DataConfigurationBuilder UseConnectionString(string connectionString)
    {
        _nameOrConnectionString = connectionString;
        return this;
    }


    /// <summary>
    /// Adds the provider factory to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="providerFactory">The provider factory.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="DbProviderFactory"/>
    public DataConfigurationBuilder AddProviderFactory<TService>(TService providerFactory)
        where TService : DbProviderFactory
    {
        _providerFactoryType = typeof(TService);
        _services.TryAddSingleton(providerFactory);
        return this;
    }

    /// <summary>
    /// Adds the provider factory to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="implementationFactory">The implementation factory.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="DbProviderFactory"/>
    public DataConfigurationBuilder AddProviderFactory<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : DbProviderFactory
    {
        _providerFactoryType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    /// <summary>
    /// Adds the provider factory to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="DbProviderFactory"/>
    public DataConfigurationBuilder AddProviderFactory<TService>()
        where TService : DbProviderFactory
    {
        _providerFactoryType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }


    /// <summary>
    /// Adds the data cache service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="dataCache">The data cache.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IDataCache"/>
    public DataConfigurationBuilder AddDataCache<TService>(TService dataCache)
        where TService : class, IDataCache
    {
        _dataCacheType = typeof(TService);
        _services.TryAddSingleton(dataCache);
        return this;
    }

    /// <summary>
    /// Adds the data cache service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="implementationFactory">The implementation factory.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IDataCache"/>
    public DataConfigurationBuilder AddDataCache<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class, IDataCache
    {
        _dataCacheType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    /// <summary>
    /// Adds the data cache service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IDataCache"/>
    public DataConfigurationBuilder AddDataCache<TService>()
        where TService : class, IDataCache
    {
        _dataCacheType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }


    /// <summary>
    /// Adds the query generator service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="queryGenerator">The query generator.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IQueryGenerator"/>
    public DataConfigurationBuilder AddQueryGenerator<TService>(TService queryGenerator)
        where TService : class, IQueryGenerator
    {
        _queryGeneratorType = typeof(TService);
        _services.TryAddSingleton(queryGenerator);
        return this;
    }

    /// <summary>
    /// Adds the query generator service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IQueryGenerator"/>
    public DataConfigurationBuilder AddQueryGenerator<TService>()
        where TService : class, IQueryGenerator
    {
        _queryGeneratorType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }

    /// <summary>
    /// Adds the query generator service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="implementationFactory">The implementation factory.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IQueryGenerator"/>
    public DataConfigurationBuilder AddQueryGenerator<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class, IQueryGenerator
    {
        _queryGeneratorType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }

    /// <summary>
    /// Adds the SQL server generator to use with this configuration.
    /// </summary>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    public DataConfigurationBuilder AddSqlServerGenerator()
    {
        AddQueryGenerator<SqlServerGenerator>();
        return this;
    }

    /// <summary>
    /// Adds the sqlite generator to use with this configuration.
    /// </summary>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    public DataConfigurationBuilder AddSqliteGenerator()
    {
        AddQueryGenerator<SqliteGenerator>();
        return this;
    }

    /// <summary>
    /// Adds the PostgreSQL generator to use with this configuration.
    /// </summary>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    public DataConfigurationBuilder AddPostgreSqlGenerator()
    {
        AddQueryGenerator<PostgreSqlGenerator>();
        return this;
    }


    /// <summary>
    /// Adds the query logger service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="queryLogger">The query logger.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IDataQueryLogger"/>
    public DataConfigurationBuilder AddQueryLogger<TService>(TService queryLogger)
        where TService : class, IDataQueryLogger
    {
        _queryLoggerType = typeof(TService);
        _services.TryAddSingleton(queryLogger);
        return this;
    }

    /// <summary>
    /// Adds the query logger service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IDataQueryLogger"/>
    public DataConfigurationBuilder AddQueryLogger<TService>()
        where TService : class, IDataQueryLogger
    {
        _queryLoggerType = typeof(TService);
        _services.TryAddSingleton<TService>();
        return this;
    }

    /// <summary>
    /// Adds the query logger service to use with this configuration.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="implementationFactory">The implementation factory.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
    /// <seealso cref="IDataQueryLogger"/>
    public DataConfigurationBuilder AddQueryLogger<TService>(Func<IServiceProvider, TService> implementationFactory)
        where TService : class, IDataQueryLogger
    {
        _queryLoggerType = typeof(TService);
        _services.TryAddSingleton(implementationFactory);
        return this;
    }


    /// <summary>
    /// Adds services via the configuration setup action.
    /// </summary>
    /// <param name="setupAction">The configuration setup action.</param>
    /// <returns>
    /// The same configuration builder so that multiple calls can be chained.
    /// </returns>
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

        _services.TryAddSingleton<IDataConfiguration>(sp =>
        {
            var connectionString = ResolveConnectionString(sp, _nameOrConnectionString);

            return new DataConfiguration(
                sp.GetRequiredService(providerFactory) as DbProviderFactory,
                connectionString,
                sp.GetService(dataCache) as IDataCache,
                sp.GetService(queryGenerator) as IQueryGenerator,
                sp.GetService(queryLogger) as IDataQueryLogger
            );
        });

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

        _services.TryAddSingleton<IDataConfiguration<TDiscriminator>>(sp =>
        {
            var connectionString = ResolveConnectionString(sp, _nameOrConnectionString);

            return new DataConfiguration<TDiscriminator>(
                sp.GetRequiredService(providerFactory) as DbProviderFactory,
                connectionString,
                sp.GetService(dataCache) as IDataCache,
                sp.GetService(queryGenerator) as IQueryGenerator,
                sp.GetService(queryLogger) as IDataQueryLogger
            );
        });

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

    private static string ResolveConnectionString(IServiceProvider serviceProvider, string nameOrConnectionString)
    {
        var isConnectionString = nameOrConnectionString.IndexOfAny([';', '=']) > 0;
        if (isConnectionString)
            return nameOrConnectionString;

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // first try connection strings section
        var connectionString = configuration.GetConnectionString(nameOrConnectionString);
        if (connectionString.HasValue())
            return connectionString;

        // next try root collection
        connectionString = configuration[nameOrConnectionString];
        if (connectionString.HasValue())
            return connectionString;

        return null;
    }
}
