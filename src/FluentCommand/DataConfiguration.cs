using System.Data.Common;

using FluentCommand.Query.Generators;

namespace FluentCommand;

/// <summary>
/// The database configuration
/// </summary>
/// <seealso cref="IDataConfiguration" />
public class DataConfiguration : IDataConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataConfiguration" /> class.
    /// </summary>
    /// <param name="providerFactory">The database provider factory.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="cache">The data cache manager.</param>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="queryLogger">The query command logger.</param>
    public DataConfiguration(
        DbProviderFactory providerFactory,
        string connectionString,
        IDataCache cache = null,
        IQueryGenerator queryGenerator = null,
        IDataQueryLogger queryLogger = null)
    {
        ProviderFactory = providerFactory;
        ConnectionString = connectionString;
        QueryLogger = queryLogger;
        DataCache = cache;
        QueryGenerator = queryGenerator ?? new SqlServerGenerator();
    }

    /// <summary>
    /// Gets the database provider factory.
    /// </summary>
    /// <value>
    /// The provider factory.
    /// </value>
    public virtual DbProviderFactory ProviderFactory { get; }

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    /// <value>
    /// The connection string.
    /// </value>
    public virtual string ConnectionString { get; }

    /// <summary>
    /// Gets the data command query logger.
    /// </summary>
    /// <value>
    /// The data command query logger.
    /// </value>
    public IDataQueryLogger QueryLogger { get; }

    /// <summary>
    /// Gets the data cache manager.
    /// </summary>
    /// <value>
    /// The data cache manager.
    /// </value>
    public virtual IDataCache DataCache { get; }

    /// <summary>
    /// Gets the query generator provider.
    /// </summary>
    /// <value>
    /// The query generator provider.
    /// </value>
    public IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Creates a new data session from this database configuration
    /// </summary>
    /// <returns>
    /// A new <see cref="IDataSession" /> instance.
    /// </returns>
    public virtual IDataSession CreateSession()
    {
        var connection = CreateConnection();
        return new DataSession(connection, true, DataCache, QueryGenerator, QueryLogger);
    }

    /// <summary>
    /// Creates a new <see cref="DbConnection" /> instance from this database configuration.
    /// </summary>
    /// <returns>
    /// A new <see cref="DbConnection" /> instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Database provider factory failed to create a connection object.
    /// or
    /// The connection string is invalid
    /// </exception>
    public virtual DbConnection CreateConnection()
    {
        var connection = ProviderFactory.CreateConnection();
        if (connection == null)
            throw new InvalidOperationException("Database provider factory failed to create a connection object.");

        if (string.IsNullOrEmpty(ConnectionString))
            throw new InvalidOperationException("The connection string is invalid");

        connection.ConnectionString = ConnectionString;
        return connection;
    }
}
